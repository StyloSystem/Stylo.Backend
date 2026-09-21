using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stylo.Backend.Stylo.Application.DTOs;
using Stylo.Backend.Stylo.Application.Exceptions;
using Stylo.Backend.Stylo.Application.Interfaces;
using Stylo.Backend.Stylo.Domain.Enums;

namespace Stylo.Backend.Stylo.API.Controllers
{
    /// <summary>
    /// Provides endpoints for placing, tracking, confirming, and canceling customer orders.
    /// </summary>
    [ApiController]
    [Route("api/orders")]
    [Authorize]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrdersController"/> class.
        /// </summary>
        /// <param name="orderService">The service used to handle order processing and retrieval.</param>
        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
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

        private bool IsAdmin() => User.IsInRole("Admin");

        /// <summary>
        /// Creates a new order using the authenticated user's cart.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(OrderDto), StatusCodes.Status201Created)]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequestDto request)
        {
            var userId = GetUserId();
            var order = await _orderService.CreateOrderFromCartAsync(userId, request);
            return CreatedAtAction(nameof(GetOrderById), new { id = order.Id }, order);
        }

        /// <summary>
        /// Returns all orders created by the authenticated user.
        /// </summary>
        [HttpGet("mine")]
        [ProducesResponseType(typeof(List<OrderDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMyOrders()
        {
            var userId = GetUserId();
            var orders = await _orderService.GetMyOrdersAsync(userId);
            return Ok(orders);
        }

        /// <summary>
        /// Returns an order by ID. Admins can access any order, while regular users can access their own orders.
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetOrderById(int id)
        {
            var userId = GetUserId();
            var order = await _orderService.GetOrderByIdAsync(userId, IsAdmin(), id);
            return Ok(order);
        }

        /// <summary>
        /// Confirms an order. Access depends on the user's authorization and order ownership.
        /// </summary>
        [HttpPut("{id}/confirm")]
        [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> ConfirmOrder(int id)
        {
            var userId = GetUserId();
            var order = await _orderService.ConfirmOrderAsync(userId, IsAdmin(), id);
            return Ok(order);
        }

        /// <summary>
        /// Cancels an order. Access depends on the user's authorization and order ownership.
        /// </summary>
        [HttpPut("{id}/cancel")]
        [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> CancelOrder(int id)
        {
            var userId = GetUserId();
            var order = await _orderService.CancelOrderAsync(userId, IsAdmin(), id);
            return Ok(order);
        }
    }
}
