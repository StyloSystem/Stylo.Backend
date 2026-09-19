using Stylo.Backend.Stylo.Application.DTOs;
using Stylo.Backend.Stylo.Application.Exceptions;
using Stylo.Backend.Stylo.Application.Interfaces;
using Stylo.Backend.Stylo.Domain.Entities;
using Stylo.Backend.Stylo.Domain.Enums;

namespace Stylo.Backend.Stylo.Application.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ICartRepository _cartRepository;

        public OrderService(IOrderRepository orderRepository, ICartRepository cartRepository)
        {
            _orderRepository = orderRepository;
            _cartRepository = cartRepository;
        }

        public async Task<OrderDto> CreateOrderFromCartAsync(int userId, CreateOrderRequestDto dto)
        {
            var createdOrder = await _orderRepository.CreateOrderFromCartTransactionAsync(userId, dto);
            return MapToDto(createdOrder);
        }

        public async Task<List<OrderDto>> GetMyOrdersAsync(int userId)
        {
            var orders = await _orderRepository.GetOrdersByUserIdAsync(userId);
            return orders.Select(MapToDto).ToList();
        }

        public async Task<OrderDto> GetOrderByIdAsync(int userId, bool isAdmin, int orderId)
        {
            var order = await _orderRepository.GetOrderByIdAsync(orderId);
            if (order == null || (!isAdmin && order.UserId != userId))
            {
                throw new NotFoundException($"Order with ID {orderId} not found.");
            }

            return MapToDto(order);
        }

        public async Task<OrderDto> ConfirmOrderAsync(int userId, int orderId)
        {
            var order = await _orderRepository.GetOrderByIdAsync(orderId);
            if (order == null || order.UserId != userId)
            {
                throw new NotFoundException($"Order with ID {orderId} not found.");
            }

            if (order.Status == OrderStatus.Cancelled)
            {
                throw new BadRequestException("Cannot confirm a cancelled order.");
            }

            order.Status = OrderStatus.Confirmed;
            foreach (var item in order.OrderItems)
            {
                if (item.Status != OrderItemStatus.Cancelled)
                {
                    item.Status = OrderItemStatus.Confirmed;
                }
            }

            RecalculateOrderTotalsAndStatus(order);
            await _orderRepository.UpdateOrderAsync(order);
            return MapToDto(order);
        }

        public async Task<OrderDto> CancelOrderAsync(int userId, int orderId)
        {
            var order = await _orderRepository.GetOrderByIdAsync(orderId);
            if (order == null || order.UserId != userId)
            {
                throw new NotFoundException($"Order with ID {orderId} not found.");
            }

            order.Status = OrderStatus.Cancelled;
            foreach (var item in order.OrderItems)
            {
                item.Status = OrderItemStatus.Cancelled;
            }

            RecalculateOrderTotalsAndStatus(order);
            await _orderRepository.UpdateOrderAsync(order);
            return MapToDto(order);
        }

        public async Task<OrderDto> UpdateOrderItemAsync(int userId, int orderId, int orderItemId, UpdateOrderItemRequestDto dto)
        {
            var order = await _orderRepository.GetOrderByIdAsync(orderId);
            if (order == null || order.UserId != userId)
            {
                throw new NotFoundException($"Order with ID {orderId} not found.");
            }

            var item = order.OrderItems.FirstOrDefault(oi => oi.Id == orderItemId);
            if (item == null)
            {
                throw new NotFoundException($"Order item with ID {orderItemId} not found in this order.");
            }

            if (dto.Quantity.HasValue && dto.Quantity.Value > 0)
            {
                item.Quantity = dto.Quantity.Value;
            }

            if (dto.Status.HasValue)
            {
                item.Status = dto.Status.Value;
            }

            RecalculateOrderTotalsAndStatus(order);
            await _orderRepository.UpdateOrderAsync(order);
            return MapToDto(order);
        }

        public async Task<List<OrderDto>> GetAllOrdersForAdminAsync()
        {
            var orders = await _orderRepository.GetAllOrdersAsync();
            return orders.Select(MapToDto).ToList();
        }

        private static void RecalculateOrderTotalsAndStatus(Order order)
        {
            var activeItems = order.OrderItems.Where(oi => oi.Status != OrderItemStatus.Cancelled).ToList();
            order.TotalPrice = activeItems.Sum(oi => oi.UnitPriceAtPurchase * oi.Quantity);

            if (order.OrderItems.Count > 0 && order.OrderItems.All(oi => oi.Status == OrderItemStatus.Cancelled))
            {
                order.Status = OrderStatus.Cancelled;
            }
            else if (activeItems.Count > 0 && activeItems.All(oi => oi.Status == OrderItemStatus.Confirmed))
            {
                order.Status = OrderStatus.Confirmed;
            }
        }

        private static OrderDto MapToDto(Order order)
        {
            var items = order.OrderItems.Select(oi => new OrderItemDto
            {
                Id = oi.Id,
                ProductId = oi.ProductId,
                ProductName = oi.Product?.Name ?? string.Empty,
                ProductImageUrl = oi.Product?.ImageUrl ?? string.Empty,
                Size = oi.Size,
                Quantity = oi.Quantity,
                UnitPriceAtPurchase = oi.UnitPriceAtPurchase,
                Status = oi.Status,
                TotalPrice = oi.UnitPriceAtPurchase * oi.Quantity
            }).ToList();

            return new OrderDto
            {
                Id = order.Id,
                UserId = order.UserId,
                RecipientName = order.RecipientName,
                ContactPhone = order.ContactPhone,
                ShippingAddress = order.ShippingAddress,
                PaymentMethod = order.PaymentMethod,
                Status = order.Status,
                TotalPrice = order.TotalPrice,
                CreatedAt = order.CreatedAt,
                Items = items
            };
        }
    }
}
