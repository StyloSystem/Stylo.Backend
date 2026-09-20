using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Stylo.Backend.Stylo.Application.DTOs;
using Stylo.Backend.Stylo.Application.Exceptions;
using Stylo.Backend.Stylo.Application.Interfaces;
using Stylo.Backend.Stylo.Application.OTP.Enums;
using Stylo.Backend.Stylo.Application.Settings;
using Stylo.Backend.Stylo.Domain.Entities;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace Stylo.Backend.Stylo.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;
        private readonly IOtpService _otpService;
        private readonly IEmailService _emailService;
        private readonly ICacheService _cacheService;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly ITokenManagerService _tokenManagerService;
        private readonly OtpSettings _otpSettings;

        public AuthService(
            IUserRepository userRepository,
            IJwtTokenGenerator jwtTokenGenerator,
            IOtpService otpService,
            IEmailService emailService,
            ICacheService cacheService,
            IPasswordHasher<User> passwordHasher,
            ITokenManagerService tokenManagerService,
            IOptions<OtpSettings> otpSettings)   
        {
            _userRepository = userRepository;
            _jwtTokenGenerator = jwtTokenGenerator;
            _otpService = otpService;
            _emailService = emailService;
            _cacheService = cacheService;
            _passwordHasher = passwordHasher;
            _tokenManagerService = tokenManagerService;
            _otpSettings = otpSettings.Value;   
        }

        // Temporary registration data stored in Redis until the OTP is verified.
        private class PendingRegistration
        {
            public string Name { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string PasswordHash { get; set; } = string.Empty;
        }

        private static string BuildPendingKey(string email) =>
            $"register:pending:{email.Trim().ToLowerInvariant()}";

        public async Task RequestRegisterOtpAsync(RegisterRequestDto dto)
        {
            if (dto == null)
                throw new BadRequestException("Request body is required.");

            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new BadRequestException("Name is required.");

            if (string.IsNullOrWhiteSpace(dto.Email) || !new EmailAddressAttribute().IsValid(dto.Email))
                throw new BadRequestException("A valid email address is required.");

            if (string.IsNullOrWhiteSpace(dto.Password) || dto.Password.Length < 6)
                throw new BadRequestException("Password must be at least 6 characters long.");

            if (dto.Password != dto.ConfirmPassword)
                throw new BadRequestException("Password and confirm password do not match.");

            if (await _userRepository.EmailExistsAsync(dto.Email))
                throw new ConflictException("Email is already registered.");

            // Hash the password before storing it in Redis, so it's never stored as plain text.
            var passwordHash = _passwordHasher.HashPassword(new User(), dto.Password);

            var pending = new PendingRegistration
            {
                Name = dto.Name.Trim(),
                Email = dto.Email.Trim(),
                PasswordHash = passwordHash
            };

            await _cacheService.SetStringAsync(
                BuildPendingKey(dto.Email),
                JsonSerializer.Serialize(pending),
                TimeSpan.FromMinutes(5));

            // Throws RateLimitExceededException automatically if the user requested
            // an OTP too recently (see OtpService's cooldown logic).
            var otp = await _otpService.GenerateAndStoreOtpAsync(dto.Email, OtpPurpose.Register);

            await _emailService.SendEmailAsync(
                dto.Email,
                "Stylo - Account Verification Code",
                $"<p>Hi {pending.Name},</p>" +
                $"<p>Your verification code is: <b>{otp}</b></p>" +
                $"<p>This code is valid for 5 minutes.</p>");
        }

        public async Task<AuthResponseDto> VerifyRegisterOtpAsync(VerifyRegisterOtpDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Otp))
                throw new BadRequestException("Email and OTP are required.");

            var verificationResult = await _otpService.VerifyOtpAsync(dto.Email, OtpPurpose.Register, dto.Otp);

            switch (verificationResult)
            {
                case OtpVerificationResult.Expired:
                    throw new BadRequestException("The verification code has expired. Please request a new one.");
                case OtpVerificationResult.MaxAttemptsExceeded:
                    throw new BadRequestException("Too many failed attempts. Please request a new verification code.");
                case OtpVerificationResult.InvalidOtp:
                    throw new BadRequestException("Invalid verification code.");
            }

            var pendingKey = BuildPendingKey(dto.Email);
            var json = await _cacheService.GetStringAsync(pendingKey);
            if (json is null)
                throw new BadRequestException("Registration data has expired. Please start the registration process again.");

            var pending = JsonSerializer.Deserialize<PendingRegistration>(json)!;

            if (await _userRepository.EmailExistsAsync(pending.Email))
                throw new ConflictException("Email is already registered.");

            var user = new User
            {
                Name = pending.Name,
                Email = pending.Email,
                PasswordHash = pending.PasswordHash,
                Role = "Customer"
            };

            await _userRepository.CreateUserWithHashedPasswordAsync(user);
            await _cacheService.RemoveAsync(pendingKey);

            var token = _jwtTokenGenerator.GenerateToken(user);

            return new AuthResponseDto
            {
                Id = user.Id.ToString(),
                Name = user.Name,
                Email = user.Email ?? string.Empty,
                Role = user.Role,
                Token = token
            };
        }

        public async Task<AuthResponseDto> LoginAsync(LoginRequestDto dto)
        {
            if (dto == null)
                throw new BadRequestException("Request body is required.");

            if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
                throw new BadRequestException("Email and password are required.");

            var user = await _userRepository.GetByEmailAsync(dto.Email);
            if (user == null)
                throw new UnauthorizedException("Invalid email or password.");

            var isValidPassword = await _userRepository.CheckPasswordAsync(user, dto.Password);
            if (!isValidPassword)
                throw new UnauthorizedException("Invalid email or password.");

            var token = _jwtTokenGenerator.GenerateToken(user);

            return new AuthResponseDto
            {
                Id = user.Id.ToString(),
                Name = user.Name,
                Email = user.Email ?? string.Empty,
                Role = user.Role,
                Token = token
            };
        }

        public async Task<UserProfileDto> GetMeAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                throw new NotFoundException("User not found.");

            return new UserProfileDto
            {
                Id = user.Id.ToString(),
                Name = user.Name,
                Email = user.Email ?? string.Empty,
                Role = user.Role
            };
        }

        private static string BuildResetTokenKey(string token) => $"reset-token:{token}";

        private static string GenerateSecureResetToken()
        {
            var bytes = System.Security.Cryptography.RandomNumberGenerator.GetBytes(32);
            return Convert.ToHexString(bytes);
        }

        public async Task ForgotPasswordAsync(ForgotPasswordDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Email))
                throw new BadRequestException("Email is required.");

            var user = await _userRepository.GetByEmailAsync(dto.Email);

            // Important: do NOT reveal whether the email exists or not.
            // If the user doesn't exist, we silently do nothing and still
            // return success from the controller, so this method just returns.
            if (user == null)
                return;

            // Throws RateLimitExceededException automatically if requested too soon.
            var otp = await _otpService.GenerateAndStoreOtpAsync(dto.Email, OtpPurpose.ResetPassword);

            await _emailService.SendEmailAsync(
                dto.Email,
                "Stylo - Password Reset Code",
                $"<p>Hi {user.Name},</p>" +
                $"<p>Your password reset code is: <b>{otp}</b></p>" +
                $"<p>This code is valid for 5 minutes. If you didn't request this, you can ignore this email.</p>");
        }

        public async Task<ResetTokenResponseDto> VerifyResetOtpAsync(VerifyResetOtpDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Otp))
                throw new BadRequestException("Email and OTP are required.");

            var verificationResult = await _otpService.VerifyOtpAsync(dto.Email, OtpPurpose.ResetPassword, dto.Otp);

            switch (verificationResult)
            {
                case OtpVerificationResult.Expired:
                    throw new BadRequestException("The verification code has expired. Please request a new one.");
                case OtpVerificationResult.MaxAttemptsExceeded:
                    throw new BadRequestException("Too many failed attempts. Please request a new verification code.");
                case OtpVerificationResult.InvalidOtp:
                    throw new BadRequestException("Invalid verification code.");
            }

            var resetToken = GenerateSecureResetToken();

            await _cacheService.SetStringAsync(
                BuildResetTokenKey(resetToken),
                dto.Email.Trim().ToLowerInvariant(),
                TimeSpan.FromMinutes(_otpSettings.ResetTokenExpirationMinutes));

            return new ResetTokenResponseDto { ResetToken = resetToken };
        }

        public async Task ResetPasswordAsync(ResetPasswordDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.ResetToken))
                throw new BadRequestException("Reset token is required.");

            if (string.IsNullOrWhiteSpace(dto.NewPassword) || dto.NewPassword.Length < 6)
                throw new BadRequestException("Password must be at least 6 characters long.");

            if (dto.NewPassword != dto.ConfirmPassword)
                throw new BadRequestException("Password and confirm password do not match.");

            var tokenKey = BuildResetTokenKey(dto.ResetToken);
            var email = await _cacheService.GetStringAsync(tokenKey);

            if (email is null)
                throw new BadRequestException("Invalid or expired reset token. Please start the password reset process again.");

            var user = await _userRepository.GetByEmailAsync(email);
            if (user == null)
                throw new NotFoundException("User not found.");

            await _userRepository.UpdatePasswordAsync(user, dto.NewPassword);

            // Invalidate the token so it can't be reused.
            await _cacheService.RemoveAsync(tokenKey);
        }

        public async Task LogoutAsync(int userId, string token)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                throw new NotFoundException("User Not Authenticated");
            }

            DateTime expiration = DateTime.UtcNow.AddHours(24);
            if (!string.IsNullOrWhiteSpace(token))
            {
                try
                {
                    var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
                    if (handler.CanReadToken(token))
                    {
                        var jwtToken = handler.ReadJwtToken(token);
                        if (jwtToken.ValidTo != DateTime.MinValue)
                        {
                            expiration = jwtToken.ValidTo;
                        }
                    }
                }
                catch
                {
                    // Fallback to default expiration
                }

                _tokenManagerService.InvalidateToken(token, expiration);
            }
        }
        
    }
}