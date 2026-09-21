using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stylo.Backend.Stylo.Application.DTOs;
using Stylo.Backend.Stylo.Application.Exceptions;
using Stylo.Backend.Stylo.Application.Interfaces;

namespace Stylo.Backend.Stylo.API.Controllers
{
    /// <summary>
    /// Provides endpoints for managing user shopping cart items and cart state.
    /// </summary>
    [ApiController]
    [Route("api/cart")]
    [Authorize]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;

        /// <summary>
        /// Initializes a new instance of the <see cref="CartController"/> class.
        /// </summary>
        /// <param name="cartService">The service used to handle shopping cart operations.</param>
        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        private int GetUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            if (!int.TryParse(userIdClaim, out var userId))
            {
                throw new UnauthorizedException("Invalid user identifier in token.");
            }
            return userId;
        }

        /// <summary>
        /// Returns the authenticated user's shopping cart.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(CartDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCart()
        {
            var userId = GetUserId();
            var cart = await _cartService.GetUserCartAsync(userId);
            return Ok(cart);
        }

        /// <summary>
        /// Adds a product to the authenticated user's shopping cart.
        /// </summary>
        [HttpPost("items")]
        [ProducesResponseType(typeof(CartDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> AddItem([FromBody] AddToCartRequestDto request)
        {
            var userId = GetUserId();
            var cart = await _cartService.AddItemToCartAsync(userId, request);
            return Ok(cart);
        }

        /// <summary>
        /// Updates the quantity of an item in the authenticated user's cart.
        /// </summary>
        [HttpPut("items/{id}")]
        [ProducesResponseType(typeof(CartDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateCartItem(int id, [FromBody] UpdateCartItemRequestDto request)
        {
            var userId = GetUserId();
            var cart = await _cartService.UpdateCartItemAsync(userId, id, request);
            return Ok(cart);
        }

        /// <summary>
        /// Removes an item from the authenticated user's shopping cart.
        /// </summary>
        [HttpDelete("items/{id}")]
        [ProducesResponseType(typeof(CartDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> RemoveItem(int id)
        {
            var userId = GetUserId();
            var cart = await _cartService.RemoveCartItemAsync(userId, id);
            return Ok(cart);
        }

        /// <summary>
        /// Removes all items from the authenticated user's shopping cart.
        /// </summary>
        [HttpDelete]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> ClearCart()
        {
            var userId = GetUserId();
            await _cartService.ClearCartAsync(userId);
            return NoContent();
        }
    }
}
