using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http;

namespace Stylo.Backend.Stylo.Application.DTOs
{
    public class ProductSizeDto
    {
        [JsonPropertyName("size")]
        public string Size { get; set; } = string.Empty;

        [JsonPropertyName("stock")]
        public int Stock { get; set; }

        [JsonPropertyName("isAvailable")]
        public bool IsAvailable { get; set; }
    }

    public class ProductCategoryDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
    }

    public class ProductDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("price")]
        public decimal Price { get; set; }

        [JsonPropertyName("imageUrl")]
        public string? ImageUrl { get; set; }

        [JsonPropertyName("imagePublicId")]
        public string? ImagePublicId { get; set; }

        [JsonPropertyName("gender")]
        public string Gender { get; set; } = string.Empty;

        [JsonPropertyName("size")]
        public string? Size { get; set; }

        [JsonPropertyName("stock")]
        public int Stock { get; set; }


        [JsonPropertyName("hasPurchased")]
        public bool HasPurchased { get; set; }

        [JsonPropertyName("category")]
        public ProductCategoryDto Category { get; set; } = null!;

        [JsonPropertyName("sizes")]
        public List<ProductSizeDto> Sizes { get; set; } = new();

        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }
    }

    public class CreateProductSizeDto
    {
        [Required(ErrorMessage = "Size is required.")]
        [JsonPropertyName("size")]
        public string Size { get; set; } = string.Empty;

        [Range(0, int.MaxValue, ErrorMessage = "Stock cannot be negative.")]
        [JsonPropertyName("stock")]
        public int Stock { get; set; }
    }

    public class CreateProductDto
    {
        [Required(ErrorMessage = "Product name is required.")]
        [StringLength(200, ErrorMessage = "Product name cannot exceed 200 characters.")]
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Product description cannot exceed 1000 characters.")]
        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than zero.")]
        [JsonPropertyName("price")]
        public decimal Price { get; set; }

        [JsonPropertyName("imageUrl")]
        public string? ImageUrl { get; set; }

        [JsonPropertyName("imagePublicId")]
        public string? ImagePublicId { get; set; }

        public IFormFile? Image { get; set; }

        [Required(ErrorMessage = "Gender is required.")]
        [JsonPropertyName("gender")]
        public string Gender { get; set; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage = "Valid Category ID is required.")]
        [JsonPropertyName("categoryId")]
        public int CategoryId { get; set; }

        [JsonPropertyName("stock")]
        public int? Stock { get; set; }

        [JsonPropertyName("sizes")]
        public List<CreateProductSizeDto> Sizes { get; set; } = new();
    }

    public class UpdateProductDto
    {
        [Required(ErrorMessage = "Product name is required.")]
        [StringLength(200, ErrorMessage = "Product name cannot exceed 200 characters.")]
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Product description cannot exceed 1000 characters.")]
        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than zero.")]
        [JsonPropertyName("price")]
        public decimal Price { get; set; }

        [JsonPropertyName("imageUrl")]
        public string? ImageUrl { get; set; }

        [JsonPropertyName("imagePublicId")]
        public string? ImagePublicId { get; set; }

        public IFormFile? Image { get; set; }

        [Required(ErrorMessage = "Gender is required.")]
        [JsonPropertyName("gender")]
        public string Gender { get; set; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage = "Valid Category ID is required.")]
        [JsonPropertyName("categoryId")]
        public int CategoryId { get; set; }

        [JsonPropertyName("stock")]
        public int? Stock { get; set; }

        [JsonPropertyName("sizes")]
        public List<CreateProductSizeDto> Sizes { get; set; } = new();
    }

    public class ProductListDto
    {
        [JsonPropertyName("items")]
        public List<ProductDto> Items { get; set; } = new();

        [JsonPropertyName("totalCount")]
        public int TotalCount { get; set; }

        [JsonPropertyName("page")]
        public int Page { get; set; }

        [JsonPropertyName("pageSize")]
        public int PageSize { get; set; }
    }
}