using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stylo.Backend.Stylo.Application.DTOs;
using Stylo.Backend.Stylo.Application.Exceptions;
using Stylo.Backend.Stylo.Application.Interfaces;

namespace Stylo.Backend.Stylo.API.Controllers
{
    /// <summary>
    /// Provides endpoints for managing user favorite products and wishlist items.
    /// </summary>
    [ApiController]
    [Route("api/favourites")]
    [Authorize]
    public class FavouriteController : ControllerBase
    {
        private readonly IFavoriteService _favoriteService;

        /// <summary>
        /// Initializes a new instance of the <see cref="FavouriteController"/> class.
        /// </summary>
        /// <param name="favoriteService">The service used to handle favorite products operations.</param>
        public FavouriteController(IFavoriteService favoriteService)
        {
            _favoriteService = favoriteService;
        }

        /// <summary>
        /// Returns all favourite products of the authenticated user.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<FavoriteDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetFavourites()
        {
            var userId = GetUserId();
            var favourites = await _favoriteService.GetUserFavoritesAsync(userId);
            return Ok(favourites);
        }

        /// <summary>
        /// Adds a product to the authenticated user's favourites.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> AddFavourite([FromBody] CreateFavoriteDto dto)
        {
            var userId = GetUserId();
            await _favoriteService.AddFavoriteAsync(userId, dto);
            return Ok(new { success = true, message = "Product added to favourites" });
        }

        /// <summary>
        /// Removes a product from the authenticated user's favourites.
        /// </summary>
        [HttpDelete("{productId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RemoveFavourite(int productId)
        {
            var userId = GetUserId();
            await _favoriteService.RemoveFavoriteAsync(userId, productId);
            return Ok(new { success = true, message = "Product removed from favourites" });
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