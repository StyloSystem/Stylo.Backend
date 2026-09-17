using Stylo.Backend.Stylo.Domain.Enums;

namespace Stylo.Backend.Stylo.Domain.Entities
{
    public class OrderItem
    {
        public int Id { get; set; }

        public int OrderId { get; set; }
        public Order Order { get; set; } = null!;

        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;

        public string Size { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPriceAtPurchase { get; set; }
        public OrderItemStatus Status { get; set; } = OrderItemStatus.Pending;
    }
}
