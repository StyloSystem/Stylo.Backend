using Stylo.Backend.Stylo.Application.DTOs;

namespace Stylo.Backend.Stylo.Application.Interfaces
{
    public interface IProductFeedbackService
    {
        Task<IEnumerable<ProductFeedbackDto>> GetByProductIdAsync(int productId);
        Task<IEnumerable<ProductFeedbackAdminDto>> GetAllAsync();
        Task<IEnumerable<FeaturedProductFeedbackDto>> GetFeaturedAsync();
        Task AddFeedbackAsync(int userId, int productId, CreateProductFeedbackDto dto);
        Task MarkAsFeaturedAsync(int id);
    }
}