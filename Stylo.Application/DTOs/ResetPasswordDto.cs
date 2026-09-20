using System.Text.Json.Serialization;

namespace Stylo.Backend.Stylo.Application.DTOs
{
    public class ForgotPasswordDto
    {
        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;
    }

    public class VerifyResetOtpDto
    {
        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        [JsonPropertyName("otp")]
        public string Otp { get; set; } = string.Empty;
    }

    public class ResetTokenResponseDto
    {
        [JsonPropertyName("resetToken")]
        public string ResetToken { get; set; } = string.Empty;
    }

    public class ResetPasswordDto
    {
        [JsonPropertyName("resetToken")]
        public string ResetToken { get; set; } = string.Empty;

        [JsonPropertyName("newPassword")]
        public string NewPassword { get; set; } = string.Empty;

        [JsonPropertyName("confirmPassword")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}