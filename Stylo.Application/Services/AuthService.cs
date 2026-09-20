using Microsoft.AspNetCore.Identity;
using Stylo.Backend.Stylo.Application.DTOs;
using Stylo.Backend.Stylo.Application.Exceptions;
using Stylo.Backend.Stylo.Application.Interfaces;
using Stylo.Backend.Stylo.Application.OTP.Enums;
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

        public AuthService(
            IUserRepository userRepository,
            IJwtTokenGenerator jwtTokenGenerator,
            IOtpService otpService,
            IEmailService emailService,
            ICacheService cacheService,
            IPasswordHasher<User> passwordHasher)
        {
            _userRepository = userRepository;
            _jwtTokenGenerator = jwtTokenGenerator;
            _otpService = otpService;
            _emailService = emailService;
            _cacheService = cacheService;
            _passwordHasher = passwordHasher;
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