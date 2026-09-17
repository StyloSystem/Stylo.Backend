using Stylo.Backend.Stylo.Application.DTOs;
using Stylo.Backend.Stylo.Domain.Enums;

namespace Stylo.Backend.Stylo.Application.Interfaces
{
    public interface IOrderService
    {
        Task<OrderDto> CreateOrderFromCartAsync(int userId, CreateOrderRequestDto dto);
        Task<List<OrderDto>> GetMyOrdersAsync(int userId);
        Task<OrderDto> GetOrderByIdAsync(int userId, bool isAdmin, int orderId);
        Task<OrderDto> ConfirmOrderAsync(int userId, int orderId);
        Task<OrderDto> CancelOrderAsync(int userId, int orderId);
        Task<OrderDto> UpdateOrderItemAsync(int userId, int orderId, int orderItemId, UpdateOrderItemRequestDto dto);
        Task<List<OrderDto>> GetAllOrdersForAdminAsync();
    }
}
