using Microsoft.EntityFrameworkCore;
using Stylo.Backend.Stylo.Application.DTOs;
using Stylo.Backend.Stylo.Application.Exceptions;
using Stylo.Backend.Stylo.Application.Interfaces;
using Stylo.Backend.Stylo.Domain.Entities;
using Stylo.Backend.Stylo.Domain.Enums;
using Stylo.Backend.Stylo.Infrastructure.Data;

namespace Stylo.Backend.Stylo.Infrastructure.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly AppDbContext _context;

        public OrderRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Order> CreateOrderFromCartTransactionAsync(int userId, CreateOrderRequestDto dto)
        {
            const int maxRetries = 3;
            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    var cart = await _context.Carts
                        .Include(c => c.CartItems)
                        .FirstOrDefaultAsync(c => c.UserId == userId);

                    if (cart == null || cart.CartItems == null || cart.CartItems.Count == 0)
                    {
                        throw new BadRequestException("Cannot create an order from an empty cart.");
                    }

                    var validCartItems = cart.CartItems.ToList();
                    var productIds = validCartItems.Select(ci => ci.ProductId).Distinct().ToList();

                    var products = await _context.Products
                        .Include(p => p.ProductSizes)
                        .Where(p => productIds.Contains(p.Id))
                        .ToListAsync();

                    var productSizeMap = new List<(CartItem CartItem, Product Product, ProductSize ProductSize)>();

                    foreach (var ci in validCartItems)
                    {
                        var product = products.FirstOrDefault(p => p.Id == ci.ProductId);
                        if (product == null || product.IsDeleted)
                        {
                            throw new BadRequestException($"Product with ID {ci.ProductId} is no longer available.");
                        }

                        if (!Enum.TryParse<Size>(ci.Size, true, out var sizeEnum))
                        {
                            throw new BadRequestException($"Invalid size '{ci.Size}' for product '{product.Name}'.", "INVALID_SIZE");
                        }

                        var productSize = product.ProductSizes.FirstOrDefault(ps => ps.Size == sizeEnum);
                        int stock = productSize?.Stock ?? 0;

                        if (ci.Quantity > stock)
                        {
                            throw new BadRequestException(
                                $"Requested quantity ({ci.Quantity}) for product '{product.Name}' (Size: {ci.Size}) is greater than available stock ({stock}).",
                                "INSUFFICIENT_STOCK");
                        }

                        if (productSize == null)
                        {
                            throw new BadRequestException(
                                $"Requested size '{ci.Size}' is not available for product '{product.Name}'.",
                                "INSUFFICIENT_STOCK");
                        }

                        productSizeMap.Add((ci, product, productSize));
                    }

                    foreach (var item in productSizeMap)
                    {
                        item.ProductSize.Stock -= item.CartItem.Quantity;
                    }

                    var orderItems = productSizeMap.Select(item => new OrderItem
                    {
                        ProductId = item.CartItem.ProductId,
                        Size = item.CartItem.Size,
                        Quantity = item.CartItem.Quantity,
                        UnitPriceAtPurchase = item.Product.Price,
                        Status = OrderItemStatus.Pending
                    }).ToList();

                    var totalPrice = orderItems.Sum(oi => oi.UnitPriceAtPurchase * oi.Quantity);

                    var order = new Order
                    {
                        UserId = userId,
                        RecipientName = dto.RecipientName,
                        ContactPhone = dto.ContactPhone,
                        ShippingAddress = dto.ShippingAddress,
                        PaymentMethod = dto.PaymentMethod,
                        Status = OrderStatus.Pending,
                        TotalPrice = totalPrice,
                        CreatedAt = DateTime.UtcNow,
                        OrderItems = orderItems
                    };

                    _context.Orders.Add(order);
                    _context.CartItems.RemoveRange(cart.CartItems);

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return await GetOrderByIdAsync(order.Id) ?? order;
                }
                catch (DbUpdateConcurrencyException)
                {
                    await transaction.RollbackAsync();
                    _context.ChangeTracker.Clear();
                    if (attempt == maxRetries)
                    {
                        throw new ConflictException("The product stock was updated concurrently by another transaction. Please try again.", "CONCURRENCY_CONFLICT");
                    }
                    await Task.Delay(50 * attempt);
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }

            throw new ConflictException("Checkout failed due to concurrent stock updates. Please try again.", "CONCURRENCY_CONFLICT");
        }

        public async Task<Order> CreateOrderAsync(Order order)
        {
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            return await GetOrderByIdAsync(order.Id) ?? order;
        }

        public async Task<List<Order>> GetOrdersByUserIdAsync(int userId)
        {
            return await _context.Orders
                .IgnoreQueryFilters()
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        }

        public async Task<Order?> GetOrderByIdAsync(int orderId)
        {
            return await _context.Orders
                .IgnoreQueryFilters()
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == orderId);
        }

        public async Task<List<Order>> GetAllOrdersAsync()
        {
            return await _context.Orders
                .IgnoreQueryFilters()
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        }

        public async Task UpdateOrderAsync(Order order)
        {
            _context.Orders.Update(order);
            await _context.SaveChangesAsync();
        }

        public async Task<Order> CancelOrderTransactionAsync(int userId, bool isAdmin, int orderId)
        {
            const int maxRetries = 3;
            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    var order = await _context.Orders
                        .Include(o => o.OrderItems)
                        .FirstOrDefaultAsync(o => o.Id == orderId);

                    if (order == null || (!isAdmin && order.UserId != userId))
                    {
                        throw new NotFoundException($"Order with ID {orderId} not found.");
                    }

                    if (order.Status != OrderStatus.Pending)
                    {
                        throw new BadRequestException("Order is no longer Pending and cannot be modified.");
                    }

                    order.Status = OrderStatus.Cancelled;

                    var productIds = order.OrderItems.Select(oi => oi.ProductId).Distinct().ToList();
                    var products = await _context.Products
                        .Include(p => p.ProductSizes)
                        .Where(p => productIds.Contains(p.Id))
                        .ToListAsync();

                    foreach (var item in order.OrderItems)
                    {
                        if (item.Status != OrderItemStatus.Cancelled)
                        {
                            item.Status = OrderItemStatus.Cancelled;

                            if (Enum.TryParse<Size>(item.Size, true, out var sizeEnum))
                            {
                                var product = products.FirstOrDefault(p => p.Id == item.ProductId);
                                if (product != null)
                                {
                                    var productSize = product.ProductSizes.FirstOrDefault(ps => ps.Size == sizeEnum);
                                    if (productSize != null)
                                    {
                                        productSize.Stock += item.Quantity;
                                    }
                                    else
                                    {
                                        product.ProductSizes.Add(new ProductSize
                                        {
                                            ProductId = item.ProductId,
                                            Size = sizeEnum,
                                            Stock = item.Quantity
                                        });
                                    }
                                }
                            }
                        }
                    }

                    var activeItems = order.OrderItems.Where(oi => oi.Status != OrderItemStatus.Cancelled).ToList();
                    order.TotalPrice = activeItems.Sum(oi => oi.UnitPriceAtPurchase * oi.Quantity);

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return await GetOrderByIdAsync(order.Id) ?? order;
                }
                catch (DbUpdateConcurrencyException)
                {
                    await transaction.RollbackAsync();
                    _context.ChangeTracker.Clear();
                    if (attempt == maxRetries)
                    {
                        throw new ConflictException("The product stock could not be updated concurrently. Please try again.", "CONCURRENCY_CONFLICT");
                    }
                    await Task.Delay(50 * attempt);
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }

            throw new ConflictException("Order cancellation failed due to concurrent stock updates. Please try again.", "CONCURRENCY_CONFLICT");
        }

        public async Task<OrderItem?> GetOrderItemByIdAsync(int orderItemId)
        {
            return await _context.OrderItems
                .IgnoreQueryFilters()
                .Include(oi => oi.Order)
                .Include(oi => oi.Product)
                .FirstOrDefaultAsync(oi => oi.Id == orderItemId);
        }

        public async Task UpdateOrderItemAsync(OrderItem orderItem)
        {
            _context.OrderItems.Update(orderItem);
            await _context.SaveChangesAsync();
        }
    }
}
