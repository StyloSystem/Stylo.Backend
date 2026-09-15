
namespace Stylo.Backend.Stylo.Domain.Entities
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; } 
        public decimal Price { get; set; }
        public string? ImageUrl { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        
        public int CategoryId { get; set; }
        public Category Category { get; set; } = null!;

        public ICollection<ProductSize> ProductSizes { get; set; } = new List<ProductSize>();
        public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public ICollection<Favourite> Favourites { get; set; } = new List<Favourite>();
        public ICollection<ProductFeedback> ProductFeedbacks { get; set; } = new List<ProductFeedback>();
    }
}
