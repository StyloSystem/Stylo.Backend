using System.Text.Json.Serialization;

namespace Stylo.Backend.Stylo.Application.DTOs
{
    public class DashboardStatsDto
    {
        [JsonPropertyName("totalOrders")]
        public int TotalOrders { get; set; }

        [JsonPropertyName("totalRevenue")]
        public decimal TotalRevenue { get; set; }

        [JsonPropertyName("totalProducts")]
        public int TotalProducts { get; set; }

        [JsonPropertyName("totalCategories")]
        public int TotalCategories { get; set; }

        [JsonPropertyName("totalCustomers")]
        public int TotalCustomers { get; set; }
    }

    public class RecentOrderDto
    {
        [JsonPropertyName("orderNumber")]
        public string OrderNumber { get; set; } = string.Empty;

        [JsonPropertyName("customerName")]
        public string CustomerName { get; set; } = string.Empty;

        [JsonPropertyName("totalPrice")]
        public decimal TotalPrice { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;
    }
}