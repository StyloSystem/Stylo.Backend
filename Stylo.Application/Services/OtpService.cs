using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Stylo.Backend.Stylo.Application.Exceptions;
using Stylo.Backend.Stylo.Application.Interfaces;
using Stylo.Backend.Stylo.Application.OTP.Enums;
using Stylo.Backend.Stylo.Application.Settings;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Stylo.Backend.Stylo.Application.Services
{
    public class OtpService : IOtpService
    {
        private readonly ICacheService _cache;
        private readonly OtpSettings _settings;
        private readonly ILogger<OtpService> _logger;

        public OtpService(ICacheService cache, IOptions<OtpSettings> options, ILogger<OtpService> logger)
        {
            _cache = cache;
            _settings = options.Value;
            _logger = logger;
        }

        private class OtpRecord
        {
            public string OtpHash { get; set; } = string.Empty;
            public int Attempts { get; set; }
            public DateTime ExpiresAtUtc { get; set; }
        }

        public async Task<string> GenerateAndStoreOtpAsync(string email, OtpPurpose purpose, CancellationToken cancellationToken = default)
        {
            var normalizedEmail = NormalizeEmail(email);
            var cooldownKey = BuildCooldownKey(purpose, normalizedEmail);

            if (await _cache.ExistsAsync(cooldownKey, cancellationToken))
            {
                throw new RateLimitExceededException("Please wait until request new OTP");
            }

            var otp = GenerateSecureOtp();
            var expiresAt = DateTime.UtcNow.AddMinutes(_settings.ExpirationMinutes);

            var record = new OtpRecord
            {
                OtpHash = HashOtp(otp, normalizedEmail, purpose),
                Attempts = 0,
                ExpiresAtUtc = expiresAt
            };

            var otpKey = BuildOtpKey(purpose, normalizedEmail);

            await _cache.SetStringAsync(otpKey, JsonSerializer.Serialize(record), TimeSpan.FromMinutes(_settings.ExpirationMinutes), cancellationToken);
            await _cache.SetStringAsync(cooldownKey, "1", TimeSpan.FromSeconds(_settings.ResendCooldownSeconds), cancellationToken);

            _logger.LogInformation("[OTP Generated] Email: {Email}, Purpose: {Purpose}, OTP: {Otp}", email, purpose, otp);

            return otp;
        }

        public async Task<OtpVerificationResult> VerifyOtpAsync(string email, OtpPurpose purpose, string inputOtp, CancellationToken cancellationToken = default)
        {
            var normalizedEmail = NormalizeEmail(email);
            var otpKey = BuildOtpKey(purpose, normalizedEmail);

            var json = await _cache.GetStringAsync(otpKey, cancellationToken);
            if (json is null)
            {
                return OtpVerificationResult.Expired;
            }

            var record = JsonSerializer.Deserialize<OtpRecord>(json)!;

            var remaining = record.ExpiresAtUtc - DateTime.UtcNow;
            if (remaining <= TimeSpan.Zero)
            {
                await _cache.RemoveAsync(otpKey, cancellationToken);
                return OtpVerificationResult.Expired;
            }

            if (record.Attempts >= _settings.MaxAttempts)
            {
                await _cache.RemoveAsync(otpKey, cancellationToken);
                return OtpVerificationResult.MaxAttemptsExceeded;
            }

            var inputHash = HashOtp(inputOtp, normalizedEmail, purpose);
            var isMatch = CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(inputHash),
                Encoding.UTF8.GetBytes(record.OtpHash));

            if (!isMatch)
            {
                record.Attempts++;

                if (record.Attempts >= _settings.MaxAttempts)
                {
                    await _cache.RemoveAsync(otpKey, cancellationToken);
                    return OtpVerificationResult.MaxAttemptsExceeded;
                }

                await _cache.SetStringAsync(otpKey, JsonSerializer.Serialize(record), remaining, cancellationToken);
                return OtpVerificationResult.InvalidOtp;
            }

            await _cache.RemoveAsync(otpKey, cancellationToken);
            return OtpVerificationResult.Success;
        }

        private string GenerateSecureOtp()
        {
            var max = (int)Math.Pow(10, _settings.Length);
            var number = RandomNumberGenerator.GetInt32(0, max);
            return number.ToString(new string('0', _settings.Length));
        }

        private string HashOtp(string otp, string email, OtpPurpose purpose)
        {
            var key = Encoding.UTF8.GetBytes(_settings.HashingSecret);
            var message = Encoding.UTF8.GetBytes($"{otp}:{email}:{purpose}");
            var hash = HMACSHA256.HashData(key, message);
            return Convert.ToHexString(hash);
        }

        private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();
        private static string BuildOtpKey(OtpPurpose purpose, string email) => $"otp:{purpose}:{email}";
        private static string BuildCooldownKey(OtpPurpose purpose, string email) => $"otp:{purpose}:{email}:cooldown";
    }
}