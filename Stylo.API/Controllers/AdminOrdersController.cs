using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stylo.Backend.Stylo.Application.DTOs;
using Stylo.Backend.Stylo.Application.Interfaces;

namespace Stylo.Backend.Stylo.API.Controllers
{
    [ApiController]
    [Route("api/admin/orders")]
    [Authorize(Roles = "Admin")]
    public class AdminOrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public AdminOrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<OrderDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllOrders()
        {
            var orders = await _orderService.GetAllOrdersForAdminAsync();
            return Ok(orders);
        }

        private int GetUserId()
        {
            var userIdClaim = User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            int.TryParse(userIdClaim, out var userId);
            return userId;
        }

        [HttpPut("{id}/confirm")]
        [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> ConfirmOrder(int id)
        {
            var order = await _orderService.ConfirmOrderAsync(GetUserId(), true, id);
            return Ok(order);
        }

        [HttpPut("{id}/cancel")]
        [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> CancelOrder(int id)
        {
            var order = await _orderService.CancelOrderAsync(GetUserId(), true, id);
            return Ok(order);
        }

        [HttpPut("{id}/items/{itemId}/confirm")]
        [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> ConfirmOrderItem(int id, int itemId)
        {
            var order = await _orderService.ConfirmOrderItemAsync(GetUserId(), true, id, itemId);
            return Ok(order);
        }

        [HttpPut("{id}/items/{itemId}/cancel")]
        [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> CancelOrderItem(int id, int itemId)
        {
            var order = await _orderService.CancelOrderItemAsync(GetUserId(), true, id, itemId);
            return Ok(order);
        }
    }
}
