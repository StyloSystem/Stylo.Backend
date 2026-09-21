using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stylo.Backend.Stylo.Application.DTOs;
using Stylo.Backend.Stylo.Application.Exceptions;
using Stylo.Backend.Stylo.Application.Interfaces;

namespace Stylo.Backend.Stylo.API.Controllers
{    /// <summary>
     /// Provides endpoints for user authentication, registration, OTP verification, and password management.
     /// </summary>
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthController"/> class.
        /// </summary>
        /// <param name="authService">The service used to handle user authentication and account actions.</param>
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Sends a verification OTP to the user's email to start the registration process.
        /// </summary>
        [HttpPost("register/request")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status429TooManyRequests)]
        public async Task<IActionResult> RequestRegisterOtp([FromBody] RegisterRequestDto request)
        {
            await _authService.RequestRegisterOtpAsync(request);
            return Ok(new { message = "A verification code has been sent to your email." });
        }

        /// <summary>
        /// Verifies the registration OTP and creates the user account.
        /// </summary>
        [HttpPost("register/verify")]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> VerifyRegisterOtp([FromBody] VerifyRegisterOtpDto request)
        {
            var result = await _authService.VerifyRegisterOtpAsync(request);
            return Ok(result);
        }

        /// <summary>
        /// Sends a password reset OTP if the email is registered.
        /// </summary>
        [HttpPost("forgot-password")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status429TooManyRequests)]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto request)
        {
            await _authService.ForgotPasswordAsync(request);
            // Always the same message, whether the email exists or not.
            return Ok(new { message = "If this email is registered, a reset code has been sent." });
        }

        /// <summary>
        /// Verifies the password reset OTP and returns a reset token.
        /// </summary>
        [HttpPost("verify-reset-otp")]
        [ProducesResponseType(typeof(ResetTokenResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> VerifyResetOtp([FromBody] VerifyResetOtpDto request)
        {
            var result = await _authService.VerifyResetOtpAsync(request);
            return Ok(result);
        }

        /// <summary>
        /// Resets the user's password using the reset token.
        /// </summary>
        [HttpPost("reset-password")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto request)
        {
            await _authService.ResetPasswordAsync(request);
            return Ok(new { message = "Password has been reset successfully." });
        }

        /// <summary>
        /// Authenticates the user and returns an access token.
        /// </summary>
        [HttpPost("login")]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            var result = await _authService.LoginAsync(request);
            return Ok(result);
        }

        /// <summary>
        /// Returns the profile of the currently authenticated user.
        /// </summary>
        [Authorize]
        [HttpGet("me")]
        [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetMe()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim, out var userId))
            {
                throw new UnauthorizedException("Invalid token user identifier.");
            }

            var result = await _authService.GetMeAsync(userId);
            return Ok(result);
        }

        /// <summary>
        /// Logs out the currently authenticated user and invalidates the current token.
        /// </summary>
        [Authorize]
        [HttpPost("logout")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Logout()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            if (!int.TryParse(userIdClaim, out var userId))
            {
                throw new UnauthorizedException("Invalid user identifier in token.");
            }

            var rawHeader = Request.Headers["Authorization"].FirstOrDefault();
            var token = string.Empty;
            if (!string.IsNullOrWhiteSpace(rawHeader) && rawHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                token = rawHeader.Substring("Bearer ".Length).Trim();
            }

            await _authService.LogoutAsync(userId, token);
            return Ok(new { success = true, message = "User Logout Successfully" });
        }
    }
}