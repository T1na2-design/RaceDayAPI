using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Data;
using RaceDay.DTOs.Users;
using RaceDay.Models;
using RaceDay.Services;

namespace RaceDayAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly IAuthService _authService;

        public UsersController(ApplicationDbContext db, IPasswordHasher<User> passwordHasher, IAuthService authService)
        {
            _db = db;
            _passwordHasher = passwordHasher;
            _authService = authService;
        }

        /// <summary>
        /// Get current user's profile.
        /// </summary>
        [HttpGet("profile")]
        [Authorize]
        public async Task<IActionResult> GetProfile()
        {
            var user = await _authService.GetCurrentUserAsync();
            if (user == null) return Unauthorized();
            return Ok(user);
        }

        /// <summary>
        /// Update current user's basic profile.
        /// </summary>
        [HttpPut("profile")]
        [Authorize]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var httpUser = HttpContext.User;
            var idClaim = httpUser.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (idClaim == null) return Unauthorized();
            if (!int.TryParse(idClaim.Value, out var userId)) return Unauthorized();

            var user = await _db.Users.FindAsync(userId);
            if (user == null) return NotFound();

            // check email uniqueness if changed
            if (user.Email != request.Email)
            {
                var exists = await _db.Users.AnyAsync(u => u.Email == request.Email && u.Id != userId);
                if (exists) return Conflict(new { message = "Email already in use." });
            }

            user.FirstName = request.FirstName;
            user.LastName = request.LastName;
            user.PhoneNumber = request.PhoneNumber;
            user.Email = request.Email;

            await _db.SaveChangesAsync();
            return Ok(new { message = "Profile updated." });
        }

        /// <summary>
        /// Change current user's password.
        /// </summary>
        [HttpPut("password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var httpUser = HttpContext.User;
            var idClaim = httpUser.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (idClaim == null) return Unauthorized();
            if (!int.TryParse(idClaim.Value, out var userId)) return Unauthorized();

            var user = await _db.Users.FindAsync(userId);
            if (user == null) return NotFound();

            var verify = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.CurrentPassword);
            if (verify == PasswordVerificationResult.Failed) return Unauthorized(new { message = "Current password is incorrect." });

            user.PasswordHash = _passwordHasher.HashPassword(user, request.NewPassword);
            await _db.SaveChangesAsync();

            return Ok(new { message = "Password changed." });
        }

        /// <summary>
        /// Get role-specific details for the current user (organiser or participant).
        /// </summary>
        [HttpGet("role-specific")]
        [Authorize]
        public async Task<IActionResult> GetRoleSpecific()
        {
            var httpUser = HttpContext.User;
            var idClaim = httpUser.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (idClaim == null) return Unauthorized();
            if (!int.TryParse(idClaim.Value, out var userId)) return Unauthorized();

            var user = await _db.Users.FindAsync(userId);
            if (user == null) return NotFound();

            if (user.Role == "Organiser")
            {
                var organiser = await _db.Organisers.Include(o => o.Events).FirstOrDefaultAsync(o => o.UserId == userId);
                return Ok(new { role = "Organiser", organiser });
            }
            else if (user.Role == "Participant")
            {
                var participant = await _db.Participants.Include(p => p.Enrolments).FirstOrDefaultAsync(p => p.UserId == userId);
                return Ok(new { role = "Participant", participant });
            }

            return Ok(new { role = user.Role });
        }
    }
}
