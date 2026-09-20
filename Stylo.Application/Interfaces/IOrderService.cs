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
        Task<OrderDto> ConfirmOrderItemAsync(int userId, bool isAdmin, int orderId, int orderItemId);
        Task<OrderDto> CancelOrderItemAsync(int userId, bool isAdmin, int orderId, int orderItemId);
        Task<OrderDto> UpdateOrderItemAsync(int userId, bool isAdmin, int orderId, int orderItemId, UpdateOrderItemRequestDto dto);
        Task<List<OrderDto>> GetAllOrdersForAdminAsync();
    }
}
