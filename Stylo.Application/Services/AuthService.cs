using System.ComponentModel.DataAnnotations;
using Stylo.Backend.Stylo.Application.DTOs;
using Stylo.Backend.Stylo.Application.Exceptions;
using Stylo.Backend.Stylo.Application.Interfaces;
using Stylo.Backend.Stylo.Domain.Entities;

namespace Stylo.Backend.Stylo.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;
        private readonly ITokenManagerService _tokenManagerService;

        public AuthService(IUserRepository userRepository, IJwtTokenGenerator jwtTokenGenerator, ITokenManagerService tokenManagerService)
        {
            _userRepository = userRepository;
            _jwtTokenGenerator = jwtTokenGenerator;
            _tokenManagerService = tokenManagerService;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto dto)
        {
            if (dto == null)
            {
                throw new BadRequestException("Request body is required.");
            }

            if (string.IsNullOrWhiteSpace(dto.Name))
            {
                throw new BadRequestException("Name is required.");
            }

            if (string.IsNullOrWhiteSpace(dto.Email) || !new EmailAddressAttribute().IsValid(dto.Email))
            {
                throw new BadRequestException("A valid email address is required.");
            }

            if (string.IsNullOrWhiteSpace(dto.Password) || dto.Password.Length < 6)
            {
                throw new BadRequestException("Password must be at least 6 characters long.");
            }

            if (dto.Password != dto.ConfirmPassword)
            {
                throw new BadRequestException("Password and confirm password do not match.");
            }

            if (await _userRepository.EmailExistsAsync(dto.Email))
            {
                throw new ConflictException("Email is already registered.");
            }

            var user = new User
            {
                Name = dto.Name.Trim(),
                Email = dto.Email.Trim(),
                UserName = dto.Email.Trim(),
                Role = "Customer"
            };

            var success = await _userRepository.CreateUserAsync(user, dto.Password);
            if (!success)
            {
                throw new BadRequestException("User registration failed.");
            }

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
            {
                throw new BadRequestException("Request body is required.");
            }

            if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
            {
                throw new BadRequestException("Email and password are required.");
            }

            var user = await _userRepository.GetByEmailAsync(dto.Email);
            if (user == null)
            {
                throw new UnauthorizedException("Invalid email or password.");
            }

            var isValidPassword = await _userRepository.CheckPasswordAsync(user, dto.Password);
            if (!isValidPassword)
            {
                throw new UnauthorizedException("Invalid email or password.");
            }

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
            {
                throw new NotFoundException("User not found.");
            }

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
