namespace Stylo.Backend.Stylo.Domain.Entities
{
    public class ProductFeedback
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;

        public int OrderId { get; set; }
        public Order Order { get; set; } = null!;

        public string Message { get; set; } = string.Empty;
        public bool IsFeatured { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
