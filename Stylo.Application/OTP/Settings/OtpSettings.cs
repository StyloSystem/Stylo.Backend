namespace Stylo.Backend.Stylo.Application.Settings
{
    public class OtpSettings
    {
        public int Length { get; set; } = 6;
        public int ExpirationMinutes { get; set; } = 5;
        public int MaxAttempts { get; set; } = 5;
        public int ResendCooldownSeconds { get; set; } = 60;
        public int ResetTokenExpirationMinutes { get; set; } = 10;
        public string HashingSecret { get; set; } = string.Empty;
    }
}