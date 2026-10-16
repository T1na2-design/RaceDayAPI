using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RaceDay.Data;
using RaceDay.DTOs.Auth;
using RaceDay.Models;

namespace RaceDay.Services
{
    public class AuthService : IAuthService
    {
        private const string OrganiserRole = "Organiser";
        private const string ParticipantRole = "Participant";

        // Used to burn the same CPU time when the email doesn't exist, so response
        // timing doesn't reveal which emails are registered.
        private static readonly User DummyUser = new();
        private static readonly string DummyHash =
            new PasswordHasher<User>().HashPassword(DummyUser, Guid.NewGuid().ToString());

        private readonly ApplicationDbContext _db;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IPasswordHasher<User> _passwordHasher;

        public AuthService(ApplicationDbContext db, IHttpContextAccessor httpContextAccessor, IPasswordHasher<User> passwordHasher)
        {
            _db = db;
            _httpContextAccessor = httpContextAccessor;
            _passwordHasher = passwordHasher;
        }

        public async Task<UserResponse> RegisterAsync(RegisterRequest request)
        {
            // Only allow known self-service roles; never trust a client-supplied role string.
            if (request.Role != OrganiserRole && request.Role != ParticipantRole)
                throw new InvalidOperationException("InvalidRole");

            var email = NormalizeEmail(request.Email);

            if (await _db.Users.AnyAsync(u => u.Email == email))
                throw new InvalidOperationException("EmailExists");

            var user = new User
            {
                Email = email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                PhoneNumber = request.PhoneNumber,
                Role = request.Role,
                DateOfBirth = request.DateOfBirth,
                Gender = request.Gender,
                IdNumber = request.IdNumber,
                EmergencyContactName = request.EmergencyContactName,
                EmergencyContactNumber = request.EmergencyContactNumber,
                MedicalConditions = request.MedicalConditions,
                ClubAffiliation = request.ClubAffiliation,
            };

            user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

            // Attach the role-specific record to the user so everything saves in ONE
            // transaction (no orphaned users if the second insert would have failed).
            if (request.Role == OrganiserRole)
                user.Organiser = new Organiser();
            else
                user.Participant = new Participant();

            _db.Users.Add(user);

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                // Two simultaneous registrations can both pass the AnyAsync check;
                // the unique index on Email catches the loser.
                _db.ChangeTracker.Clear();
                if (await _db.Users.AnyAsync(u => u.Email == email))
                    throw new InvalidOperationException("EmailExists");
                throw;
            }

            StartSession(user);
            return MapToResponse(user);
        }

        public async Task<UserResponse> LoginAsync(LoginRequest request)
        {
            var email = NormalizeEmail(request.Email);
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);

            var result = _passwordHasher.VerifyHashedPassword(
                user ?? DummyUser,
                user?.PasswordHash ?? DummyHash,
                request.Password);

            if (user == null || result == PasswordVerificationResult.Failed)
                throw new UnauthorizedAccessException("InvalidCredentials");

            // Upgrade the stored hash if the hasher's settings have been strengthened.
            if (result == PasswordVerificationResult.SuccessRehashNeeded)
            {
                user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);
                await _db.SaveChangesAsync();
            }

            StartSession(user);
            return MapToResponse(user);
        }

        public Task LogoutAsync()
        {
            GetHttpContext().Session.Clear();
            return Task.CompletedTask;
        }

        public async Task<UserResponse> GetCurrentUserAsync()
        {
            var id = GetHttpContext().Session.GetInt32(SessionAuthenticationHandler.SessionUserIdKey);
            if (!id.HasValue)
                return null;

            var user = await _db.Users.FindAsync(id.Value);
            if (user == null) return null;
            return MapToResponse(user);
        }

        private void StartSession(User user)
        {
            var session = GetHttpContext().Session;
            session.Clear(); // drop anything left over from a previous identity
            session.SetInt32(SessionAuthenticationHandler.SessionUserIdKey, user.Id);
            session.SetString(SessionAuthenticationHandler.SessionUserRoleKey, user.Role);
        }

        private HttpContext GetHttpContext() =>
            _httpContextAccessor.HttpContext
            ?? throw new InvalidOperationException("No active HttpContext.");

        private static string NormalizeEmail(string? email) =>
            (email ?? string.Empty).Trim().ToLowerInvariant();

        private static UserResponse MapToResponse(User user)
        {
            return new UserResponse
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                Role = user.Role,
                DateOfBirth = user.DateOfBirth,
                Gender = user.Gender,
                IdNumber = user.IdNumber,
                EmergencyContactName = user.EmergencyContactName,
                EmergencyContactNumber = user.EmergencyContactNumber,
                MedicalConditions = user.MedicalConditions,
                ClubAffiliation = user.ClubAffiliation
            };
        }
    }
}