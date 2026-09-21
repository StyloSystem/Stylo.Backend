using Stylo.Backend.Stylo.Application.DTOs;
using Stylo.Backend.Stylo.Domain.Entities;

namespace Stylo.Backend.Stylo.Application.Interfaces
{
    public interface IOrderRepository
    {
        Task<Order> CreateOrderFromCartTransactionAsync(int userId, CreateOrderRequestDto dto);
        Task<Order> CreateOrderAsync(Order order);
        Task<List<Order>> GetOrdersByUserIdAsync(int userId);
        Task<Order?> GetOrderByIdAsync(int orderId);
        Task<List<Order>> GetAllOrdersAsync();
        Task UpdateOrderAsync(Order order);
        Task<Order> CancelOrderTransactionAsync(int userId, bool isAdmin, int orderId);
        Task<OrderItem?> GetOrderItemByIdAsync(int orderItemId);
        Task UpdateOrderItemAsync(OrderItem orderItem);
    }
}
