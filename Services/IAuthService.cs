using RaceDay.DTOs.Auth;

namespace RaceDay.Services
{
    public interface IAuthService
    {
        Task<UserResponse> RegisterAsync(RegisterRequest request);
        Task<UserResponse> LoginAsync(LoginRequest request);
        Task LogoutAsync();
        Task<UserResponse> GetCurrentUserAsync();
    }
}
