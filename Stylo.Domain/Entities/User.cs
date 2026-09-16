using Microsoft.AspNetCore.Identity;

namespace Stylo.Backend.Stylo.Domain.Entities
{
    public class User : IdentityUser<int>
    {
        public string Name { get; set; } = string.Empty;
        public string Role { get; set; } = "Customer";

        public Cart? Cart { get; set; }
        public ICollection<Order> Orders { get; set; } = new List<Order>();
        public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
        public ICollection<ProductFeedback> ProductFeedbacks { get; set; } = new List<ProductFeedback>();
        public ICollection<WebsiteFeedback> WebsiteFeedbacks { get; set; } = new List<WebsiteFeedback>();
    }
}
