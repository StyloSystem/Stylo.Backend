using Stylo.Backend.Stylo.Application.OTP.Enums;

namespace Stylo.Backend.Stylo.Application.Interfaces
{
    public interface IOtpService
    {
        Task<string> GenerateAndStoreOtpAsync(string email, OtpPurpose purpose, CancellationToken cancellationToken = default);
        Task<OtpVerificationResult> VerifyOtpAsync(string email, OtpPurpose purpose, string inputOtp, CancellationToken cancellationToken = default);
    }
}