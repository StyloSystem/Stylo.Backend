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
    }
}
