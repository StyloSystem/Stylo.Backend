using Stylo.Backend.Stylo.Application.DTOs;

namespace Stylo.Backend.Stylo.Application.Interfaces
{
    public interface IAuthService
    {
        Task RequestRegisterOtpAsync(RegisterRequestDto dto);
        Task<AuthResponseDto> VerifyRegisterOtpAsync(VerifyRegisterOtpDto dto);
        Task<AuthResponseDto> LoginAsync(LoginRequestDto dto);
        Task<UserProfileDto> GetMeAsync(int userId);
        Task LogoutAsync(int userId, string token);
        Task ForgotPasswordAsync(ForgotPasswordDto dto);
        Task<ResetTokenResponseDto> VerifyResetOtpAsync(VerifyResetOtpDto dto);
        Task ResetPasswordAsync(ResetPasswordDto dto);
    }
}
