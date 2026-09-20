using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stylo.Backend.Stylo.Application.DTOs;
using Stylo.Backend.Stylo.Application.Exceptions;
using Stylo.Backend.Stylo.Application.Interfaces;
using Stylo.Backend.Stylo.Domain.Enums;

namespace Stylo.Backend.Stylo.API.Controllers
{
    [ApiController]
    [Route("api/orders")]
    [Authorize]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;

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

        [HttpPost]
        [ProducesResponseType(typeof(OrderDto), StatusCodes.Status201Created)]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequestDto request)
        {
            var userId = GetUserId();
            var order = await _orderService.CreateOrderFromCartAsync(userId, request);
            return CreatedAtAction(nameof(GetOrderById), new { id = order.Id }, order);
        }

        [HttpGet("mine")]
        [ProducesResponseType(typeof(List<OrderDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMyOrders()
        {
            var userId = GetUserId();
            var orders = await _orderService.GetMyOrdersAsync(userId);
            return Ok(orders);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetOrderById(int id)
        {
            var userId = GetUserId();
            var order = await _orderService.GetOrderByIdAsync(userId, IsAdmin(), id);
            return Ok(order);
        }

        [HttpPut("{id}/confirm")]
        [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> ConfirmOrder(int id)
        {
            var userId = GetUserId();
            var order = await _orderService.ConfirmOrderAsync(userId, IsAdmin(), id);
            return Ok(order);
        }

        [HttpPut("{id}/cancel")]
        [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> CancelOrder(int id)
        {
            var userId = GetUserId();
            var order = await _orderService.CancelOrderAsync(userId, IsAdmin(), id);
            return Ok(order);
        }

        [HttpPut("{id}/items/{itemId}")]
        [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateOrderItem(int id, int itemId, [FromBody] UpdateOrderItemRequestDto request)
        {
            var userId = GetUserId();
            var order = await _orderService.UpdateOrderItemAsync(userId, IsAdmin(), id, itemId, request);
            return Ok(order);
        }

        [HttpPut("{id}/items/{itemId}/confirm")]
        [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> ConfirmOrderItem(int id, int itemId)
        {
            var userId = GetUserId();
            var order = await _orderService.ConfirmOrderItemAsync(userId, IsAdmin(), id, itemId);
            return Ok(order);
        }

        [HttpPut("{id}/items/{itemId}/cancel")]
        [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> CancelOrderItem(int id, int itemId)
        {
            var userId = GetUserId();
            var order = await _orderService.CancelOrderItemAsync(userId, IsAdmin(), id, itemId);
            return Ok(order);
        }
    }
}
