namespace Stylo.Backend.Stylo.Application.OTP.Enums
{
    public enum OtpVerificationResult
    {
        Success,
        InvalidOtp,
        Expired,
        MaxAttemptsExceeded
    }
}