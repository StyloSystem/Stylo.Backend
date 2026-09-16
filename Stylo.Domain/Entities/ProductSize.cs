namespace Stylo.Backend.Stylo.Domain.Entities
{
    public class ProductSize
    {
        public int Id { get; set; }

        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;

        public string Size { get; set; } = string.Empty;
        public int Stock { get; set; }

        public bool IsAvailable => Stock > 0;
    }
}
