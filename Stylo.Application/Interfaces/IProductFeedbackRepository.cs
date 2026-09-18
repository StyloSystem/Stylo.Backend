using Stylo.Backend.Stylo.Domain.Entities;

namespace Stylo.Backend.Stylo.Application.Interfaces
{
    public interface IProductFeedbackRepository
    {
        Task<ProductFeedback?> GetByIdAsync(int id);
        Task<IEnumerable<ProductFeedback>> GetByProductIdAsync(int productId);
        Task<IEnumerable<ProductFeedback>> GetAllAsync();
        Task<IEnumerable<ProductFeedback>> GetFeaturedAsync();
        Task<int?> GetUserOrderIdForProductAsync(int userId, int productId);
        Task<bool> HasUserPurchasedProductAsync(int userId, int productId);
        Task<bool> HasUserAlreadyReviewedAsync(int userId, int productId);
        Task<bool> ProductExistsAsync(int productId);
        Task AddAsync(ProductFeedback feedback);
        Task SaveChangesAsync();
    }
}