using Stylo.Backend.Stylo.Domain.Entities;

namespace Stylo.Backend.Stylo.Application.Interfaces
{
    public interface IProductRepository
    {
        Task<List<Product>> GetAllAsync(
            int page,
            int pageSize,
            int? categoryId = null,
            string? gender = null);

        Task<int> GetCountAsync(
            int? categoryId = null,
            string? gender = null);

        Task<Product?> GetByIdAsync(int id);

        Task<bool> CategoryExistsAsync(int categoryId);

        Task<bool> HasCartItemsAsync(int productId);

        Task<bool> HasOrderItemsAsync(int productId);

        Task<bool> HasProductFeedbacksAsync(int productId);

        Task<bool> HasPurchasedProductIdsAsync(
            int? userId,
            int productId);

        Task AddAsync(Product product);

        Task UpdateAsync(Product product);

        Task DeleteAsync(Product product);
    }
}
