using Stylo.Backend.Stylo.Domain.Enums;

namespace Stylo.Backend.Stylo.Domain.Entities
{
    public class Order
    {
        public int Id { get; set; }

       
        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public decimal TotalPrice { get; set; }
        public OrderStatus OrderStatus { get; set; }
        public string RecipientName { get; set; } = string.Empty;
        public string ContactPhone { get; set; } = string.Empty;
        public string ShippingAddress { get; set; } = string.Empty;
        public PaymentMethod? PaymentMethod { get; set; } 
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
