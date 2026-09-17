using Stylo.Backend.Stylo.Domain.Entities;

namespace Stylo.Backend.Stylo.Application.Interfaces
{
    public interface IWebsiteFeedbackRepository
    {
        Task<WebsiteFeedback?> GetByIdAsync(int id);
        Task<IEnumerable<WebsiteFeedback>> GetAllAsync();
        Task<IEnumerable<WebsiteFeedback>> GetFeaturedAsync();
        Task AddAsync(WebsiteFeedback feedback);
        Task SaveChangesAsync();
    }
}
