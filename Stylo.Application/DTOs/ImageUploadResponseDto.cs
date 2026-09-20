using System.Text.Json.Serialization;

namespace Stylo.Backend.Stylo.Application.DTOs
{
    public class ImageUploadResponseDto
    {
        [JsonPropertyName("imageUrl")]
        public string ImageUrl { get; set; } = string.Empty;

        [JsonPropertyName("imagePublicId")]
        public string ImagePublicId { get; set; } = string.Empty;
    }
}
