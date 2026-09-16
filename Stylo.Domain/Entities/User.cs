using Stylo.Backend.Stylo.Domain.Enums;

namespace Stylo.Backend.Stylo.Domain.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public UserRole Role { get; set; }

        
        public Cart? Cart { get; set; }
        public ICollection<Order> Orders { get; set; } = new List<Order>();
        public ICollection<Favourite> Favourites { get; set; } = new List<Favourite>();
        public ICollection<ProductFeedback> ProductFeedbacks { get; set; } = new List<ProductFeedback>();
        public ICollection<WebsiteFeedback> WebsiteFeedbacks { get; set; } = new List<WebsiteFeedback>();
    }
}
