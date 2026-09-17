using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Stylo.Backend.Stylo.Application.DTOs
{
    public class CreateFavoriteDto
    {
        [Required(ErrorMessage = "ProductId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "ProductId must be greater than zero.")]
        [JsonPropertyName("productId")]
        public int ProductId { get; set; }
    }

    public class FavoriteDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("productId")]
        public int ProductId { get; set; }

        [JsonPropertyName("productName")]
        public string ProductName { get; set; } = string.Empty;

        [JsonPropertyName("productImageUrl")]
        public string ProductImageUrl { get; set; } = string.Empty;

        [JsonPropertyName("productPrice")]
        public decimal ProductPrice { get; set; }
    }
}