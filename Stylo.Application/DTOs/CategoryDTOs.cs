using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Stylo.Backend.Stylo.Application.DTOs
{
    public class CategoryDto
    {
        [JsonPropertyName("id")]
        [Range(1, int.MaxValue, ErrorMessage = "ID must be greater than zero.")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
    }

    public class CreateCategoryDto
    {
        [Required(ErrorMessage = "Category name is required.")]
        [StringLength(100, ErrorMessage = "Category name cannot exceed 100 characters.")]
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
    }

    public class UpdateCategoryDto
    {
        [Required(ErrorMessage = "Category name is required.")]
        [StringLength(100, ErrorMessage = "Category name cannot exceed 100 characters.")]
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
    }
}
