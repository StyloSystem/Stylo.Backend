namespace Stylo.Backend.Stylo.Application.DTOs
{
    public class FavoriteDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ProductImageUrl { get; set; } = string.Empty;
        public decimal ProductPrice { get; set; }
    }

    public class CreateFavoriteDto
    {
        public int ProductId { get; set; }
    }
}
