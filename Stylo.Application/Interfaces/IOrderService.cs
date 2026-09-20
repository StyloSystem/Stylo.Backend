using Stylo.Backend.Stylo.Application.DTOs;
using Stylo.Backend.Stylo.Domain.Enums;

namespace Stylo.Backend.Stylo.Application.Interfaces
{
    public interface IOrderService
    {
        Task<OrderDto> CreateOrderFromCartAsync(int userId, CreateOrderRequestDto dto);
        Task<List<OrderDto>> GetMyOrdersAsync(int userId);
        Task<OrderDto> GetOrderByIdAsync(int userId, bool isAdmin, int orderId);
        Task<OrderDto> ConfirmOrderAsync(int userId, bool isAdmin, int orderId);
        Task<OrderDto> CancelOrderAsync(int userId, bool isAdmin, int orderId);
        Task<List<OrderDto>> GetAllOrdersForAdminAsync();
    }
}
