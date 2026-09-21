using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stylo.Backend.Stylo.Application.DTOs;
using Stylo.Backend.Stylo.Application.Exceptions;
using Stylo.Backend.Stylo.Application.Interfaces;

namespace Stylo.Backend.Stylo.API.Controllers
{
    /// <summary>
    /// Provides endpoints for managing product reviews, customer feedback, and featured reviews.
    /// </summary>
    [ApiController]
    [Route("api")]
    public class ProductFeedbackController : ControllerBase
    {
        private readonly IProductFeedbackService _feedbackService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProductFeedbackController"/> class.
        /// </summary>
        /// <param name="feedbackService">The service used to handle product feedback operations.</param>
        public ProductFeedbackController(IProductFeedbackService feedbackService)
        {
            _feedbackService = feedbackService;
        }

        /// <summary>
        /// Submits feedback for a product. Authentication is required.
        /// </summary>
        [HttpPost("products/{productId}/feedback")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> CreateFeedback(int productId, [FromBody] CreateProductFeedbackDto dto)
        {
            var userId = GetUserId();
            await _feedbackService.AddFeedbackAsync(userId, productId, dto);
            return Ok(new { success = true, message = "Feedback submitted successfully" });
        }

        /// <summary>
        /// Returns all feedback submitted for a specific product.
        /// </summary>
        [HttpGet("products/{productId}/feedback")]
        [ProducesResponseType(typeof(IEnumerable<ProductFeedbackDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetProductFeedback(int productId)
        {
            var feedbacks = await _feedbackService.GetByProductIdAsync(productId);
            return Ok(feedbacks);
        }

        /// <summary>
        /// Returns all product feedback for administration.
        /// </summary>
        [HttpGet("admin/product-feedback")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(IEnumerable<ProductFeedbackAdminDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetAllFeedback()
        {
            var feedbacks = await _feedbackService.GetAllAsync();
            return Ok(feedbacks);
        }

        /// <summary>
        /// Marks a product feedback as featured. Admin access is required.
        /// </summary>
        [HttpPut("admin/product-feedback/{id}/feature")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> MarkAsFeatured(int id)
        {
            await _feedbackService.MarkAsFeaturedAsync(id);
            return Ok(new { success = true, message = "Feedback marked as featured" });
        }

        /// <summary>
        /// Returns all featured product feedback.
        /// </summary>
        [HttpGet("products/reviews/featured")]
        [ProducesResponseType(typeof(IEnumerable<FeaturedProductFeedbackDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetFeaturedReviews()
        {
            var feedbacks = await _feedbackService.GetFeaturedAsync();
            return Ok(feedbacks);
        }

        private int GetUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim, out var userId))
            {
                throw new UnauthorizedException("Invalid token user identifier.");
            }
            return userId;
        }
    }
}