using System.ComponentModel.DataAnnotations;

namespace Stylo.Backend.Stylo.Application.DTOs
{
    public class CartItemDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ProductImageUrl { get; set; } = string.Empty;
        public string Size { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
    }

    public class CartDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public List<CartItemDto> Items { get; set; } = new();
        public decimal GrandTotal { get; set; }
    }

    public class AddToCartRequestDto
    {
        [Required]
        public int ProductId { get; set; }

        [Required]
        public string Size { get; set; } = string.Empty;

        [Range(1, 1000)]
        public int Quantity { get; set; } = 1;
    }

    public class UpdateCartItemRequestDto
    {
        public string? Size { get; set; }

        [Range(1, 1000)]
        public int? Quantity { get; set; }
    }
}
