using Stylo.Backend.Stylo.Application.DTOs;

namespace Stylo.Backend.Stylo.Application.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterRequestDto dto);
        Task<AuthResponseDto> LoginAsync(LoginRequestDto dto);
        Task<UserProfileDto> GetMeAsync(int userId);
        Task LogoutAsync(int userId, string token);
    }
}
