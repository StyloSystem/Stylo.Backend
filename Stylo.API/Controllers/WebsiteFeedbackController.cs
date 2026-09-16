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
        public async Task<IActionResult> CreateFeedback([FromBody] CreateWebsiteFeedbackDto dto)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim, out var userId))
            {
                throw new UnauthorizedException("Invalid token user identifier.");
            }
            await _feedbackService.AddFeedbackAsync(userId, dto);
            return Ok(new { success = true, message = "Feedback submitted successfully" });
        }

        [HttpGet("admin/feedback")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllFeedback()
        {
            var feedbacks = await _feedbackService.GetAllAsync();
            return Ok(feedbacks);
        }

        [HttpPut("admin/feedback/{id}/feature")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> MarkAsFeatured(int id)
        {
            await _feedbackService.MarkAsFeaturedAsync(id);
            return Ok(new { success = true, message = "Feedback marked as featured" });
        }

        [HttpGet("feedback/featured")]
        public async Task<IActionResult> GetFeaturedFeedback()
        {
            var feedbacks = await _feedbackService.GetFeaturedAsync();
            return Ok(feedbacks);
        }
    }
}
