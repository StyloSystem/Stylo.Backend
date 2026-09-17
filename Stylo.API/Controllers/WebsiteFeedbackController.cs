using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stylo.Backend.Stylo.Application.DTOs;
using Stylo.Backend.Stylo.Application.Exceptions;
using Stylo.Backend.Stylo.Application.Interfaces;
using System.Security.Claims;

namespace Stylo.Backend.Stylo.API.Controllers
{
    [ApiController]
    [Route("api")]
    public class WebsiteFeedbackController : ControllerBase
    {
        private readonly IWebsiteFeedbackService _feedbackService;

        public WebsiteFeedbackController(IWebsiteFeedbackService feedbackService)
        {
            _feedbackService = feedbackService;
        }

        [HttpPost("feedback")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> CreateFeedback([FromBody] CreateWebsiteFeedbackDto dto)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim, out var userId))
            {
                throw new UnauthorizedException("Invalid token user identifier");
            }

            await _feedbackService.AddFeedbackAsync(userId, dto);
            return Ok(new { success = true, message = "Feedback submitted successfully" });
        }

        [HttpGet("feedback/featured")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(List<WebsiteFeedbackDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetFeaturedFeedback()
        {
            var feedbacks = await _feedbackService.GetFeaturedAsync();
            return Ok(feedbacks);
        }

        [HttpGet("admin/feedback")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(List<WebsiteFeedbackDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetAllFeedback()
        {
            var feedbacks = await _feedbackService.GetAllAsync();
            return Ok(feedbacks);
        }

        [HttpPut("admin/feedback/{id}/feature")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> MarkAsFeatured(int id)
        {
            await _feedbackService.MarkAsFeaturedAsync(id);
            return Ok(new { success = true, message = "Feedback marked as featured" });
        }
    }
}