using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Stylo.Backend.Stylo.Application.DTOs
{
    public class CreateWebsiteFeedbackDto
    {
        [Required(ErrorMessage = "Message is required")]
        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;
    }

    public class WebsiteFeedbackDto
    {
        [JsonPropertyName("id")]
        [Range(1, int.MaxValue, ErrorMessage = "ID must be greater than zero")]
        public int Id { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;

        [JsonPropertyName("userName")]
        public string UserName { get; set; } = string.Empty;

        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }
    }

    public class WebsiteFeedbackAdminDto
    {
        [JsonPropertyName("id")]
        [Range(1, int.MaxValue, ErrorMessage = "ID must be greater than zero.")]
        public int Id { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;

        [JsonPropertyName("isFeatured")]
        public bool IsFeatured { get; set; }

        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("userId")]
        [Range(1, int.MaxValue, ErrorMessage = "UserId must be greater than zero.")]
        public int UserId { get; set; }

        [JsonPropertyName("userName")]
        public string UserName { get; set; } = string.Empty;
    }
}