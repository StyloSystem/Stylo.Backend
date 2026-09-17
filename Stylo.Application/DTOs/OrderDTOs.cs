using System.ComponentModel.DataAnnotations;
using Stylo.Backend.Stylo.Domain.Enums;

namespace Stylo.Backend.Stylo.Application.DTOs
{
    public class OrderItemDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ProductImageUrl { get; set; } = string.Empty;
        public string Size { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPriceAtPurchase { get; set; }
        public OrderItemStatus Status { get; set; }
        public decimal TotalPrice { get; set; }
    }

    public class OrderDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string RecipientName { get; set; } = string.Empty;
        public string ContactPhone { get; set; } = string.Empty;
        public string ShippingAddress { get; set; } = string.Empty;
        public PaymentMethod PaymentMethod { get; set; }
        public OrderStatus Status { get; set; }
        public decimal TotalPrice { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<OrderItemDto> Items { get; set; } = new();
    }

    public class CreateOrderRequestDto
    {
        [Required]
        [MaxLength(100)]
        public string RecipientName { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string ContactPhone { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string ShippingAddress { get; set; } = string.Empty;

        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.CashOnDelivery;
    }

    public class UpdateOrderItemRequestDto
    {
        [Range(1, 1000)]
        public int? Quantity { get; set; }
        public OrderItemStatus? Status { get; set; }
    }

    public class UpdateOrderStatusRequestDto
    {
        [Required]
        public OrderStatus Status { get; set; }
    }
}
