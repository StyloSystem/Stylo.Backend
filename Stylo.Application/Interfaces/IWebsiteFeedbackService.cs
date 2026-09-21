using Stylo.Backend.Stylo.Application.DTOs;

namespace Stylo.Backend.Stylo.Application.Interfaces
{
    public interface IWebsiteFeedbackService
    {
        Task<IEnumerable<WebsiteFeedbackDto>> GetFeaturedAsync();
        Task<IEnumerable<WebsiteFeedbackAdminDto>> GetAllAsync();
        Task AddFeedbackAsync(int userId, CreateWebsiteFeedbackDto dto);
        Task MarkAsFeaturedAsync(int id);

        Task UnfeatureAsync(int id);
    }
}
