using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RaceDay.Data;
using RaceDay.DTOs.Auth;
using RaceDay.Models;

namespace RaceDay.Services
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _db;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IPasswordHasher<User> _passwordHasher;
        private const string SessionUserId = "UserId";
        private const string SessionUserRole = "UserRole";

        public AuthService(ApplicationDbContext db, IHttpContextAccessor httpContextAccessor, IPasswordHasher<User> passwordHasher)
        {
            _db = db;
            _httpContextAccessor = httpContextAccessor;
            _passwordHasher = passwordHasher;
        }

        public async Task<UserResponse> RegisterAsync(RegisterRequest request)
        {
            // check existing
            var exists = await _db.Users.AnyAsync(u => u.Email == request.Email);
            if (exists)
                throw new InvalidOperationException("EmailExists");

            var user = new User
            {
                Email = request.Email,
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

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            // create role-specific record
            if (request.Role == "Organiser")
            {
                var org = new Organiser { UserId = user.Id };
                _db.Organisers.Add(org);
                await _db.SaveChangesAsync();
            }
            else if (request.Role == "Participant")
            {
                var part = new Participant { UserId = user.Id };
                _db.Participants.Add(part);
                await _db.SaveChangesAsync();
            }

            // start session
            var http = _httpContextAccessor.HttpContext;
            http.Session.SetInt32(SessionUserId, user.Id);
            http.Session.SetString(SessionUserRole, user.Role);

            return MapToResponse(user);
        }

        public async Task<UserResponse> LoginAsync(LoginRequest request)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null)
                throw new UnauthorizedAccessException("InvalidCredentials");

            var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
            if (result == PasswordVerificationResult.Failed)
                throw new UnauthorizedAccessException("InvalidCredentials");

            var http = _httpContextAccessor.HttpContext;
            http.Session.SetInt32(SessionUserId, user.Id);
            http.Session.SetString(SessionUserRole, user.Role);

            return MapToResponse(user);
        }

        public Task LogoutAsync()
        {
            var http = _httpContextAccessor.HttpContext;
            http.Session.Remove(SessionUserId);
            http.Session.Remove(SessionUserRole);
            return Task.CompletedTask;
        }

        public async Task<UserResponse> GetCurrentUserAsync()
        {
            var http = _httpContextAccessor.HttpContext;
            var id = http.Session.GetInt32(SessionUserId);
            if (!id.HasValue)
                return null;

            var user = await _db.Users.FindAsync(id.Value);
            if (user == null) return null;
            return MapToResponse(user);
        }

        private UserResponse MapToResponse(User user)
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
