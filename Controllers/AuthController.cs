using Microsoft.AspNetCore.Mvc;
using RaceDay.DTOs.Auth;
using RaceDay.Services;
using Swashbuckle.AspNetCore.Annotations;

namespace RaceDayAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Register a new user and start a session.
        /// </summary>
        [HttpPost("register")]
        [SwaggerOperation(Summary = "Register a user and start session")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var user = await _authService.RegisterAsync(request);
                return CreatedAtAction(nameof(Me), user);
            }
            catch (InvalidOperationException ex) when (ex.Message == "EmailExists")
            {
                return Conflict(new { message = "Email already registered." });
            }
        }

        /// <summary>
        /// Login and start session.
        /// </summary>
        [HttpPost("login")]
        [SwaggerOperation(Summary = "Login and start session")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var user = await _authService.LoginAsync(request);
                return Ok(user);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { message = "Invalid email or password." });
            }
        }

        /// <summary>
        /// Logout current user and clear session.
        /// </summary>
        [HttpPost("logout")]
        [SwaggerOperation(Summary = "Logout and clear session")]
        public async Task<IActionResult> Logout()
        {
            await _authService.LogoutAsync();
            return Ok(new { message = "Logged out." });
        }

        /// <summary>
        /// Get current authenticated user profile.
        /// </summary>
        [HttpGet("me")]
        [SwaggerOperation(Summary = "Get current user profile")]
        public async Task<IActionResult> Me()
        {
            var user = await _authService.GetCurrentUserAsync();
            if (user == null) return Unauthorized();
            return Ok(user);
        }
    }
}
