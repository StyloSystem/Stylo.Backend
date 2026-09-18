using Microsoft.EntityFrameworkCore;
using Stylo.Backend.Stylo.Application.Interfaces;
using Stylo.Backend.Stylo.Domain.Entities;
using Stylo.Backend.Stylo.Domain.Enums;
using Stylo.Backend.Stylo.Infrastructure.Data;

namespace Stylo.Backend.Stylo.Infrastructure.Repositories
{
    public class ProductFeedbackRepository : IProductFeedbackRepository
    {
        private readonly AppDbContext _context;

        public ProductFeedbackRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ProductFeedback?> GetByIdAsync(int id)
        {
            return await _context.ProductFeedbacks.FindAsync(id);
        }

        public async Task<IEnumerable<ProductFeedback>> GetByProductIdAsync(int productId)
        {
            return await _context.ProductFeedbacks
                .Include(f => f.User)
                .Where(f => f.ProductId == productId)
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<ProductFeedback>> GetAllAsync()
        {
            return await _context.ProductFeedbacks
                .Include(f => f.User)
                .Include(f => f.Product)
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<ProductFeedback>> GetFeaturedAsync()
        {
            return await _context.ProductFeedbacks
                .Include(f => f.User)
                .Include(f => f.Product)
                .Where(f => f.IsFeatured)
                .ToListAsync();
        }
        public async Task<int?> GetUserOrderIdForProductAsync(int userId, int productId)
        {
            return await _context.OrderItems
                .Where(oi => oi.ProductId == productId
                          && oi.Order.UserId == userId
                          && oi.Order.Status == OrderStatus.Confirmed)
                .Select(oi => (int?)oi.OrderId)
                .FirstOrDefaultAsync();
        }
        public async Task<bool> HasUserPurchasedProductAsync(int userId, int productId)
        {
            return await _context.OrderItems
                .Include(oi => oi.Order)
                .AnyAsync(oi => oi.ProductId == productId
                             && oi.Order.UserId == userId
                             && oi.Order.Status == OrderStatus.Confirmed);
        }

        public async Task<bool> HasUserAlreadyReviewedAsync(int userId, int productId)
        {
            return await _context.ProductFeedbacks
                .AnyAsync(f => f.UserId == userId && f.ProductId == productId);
        }

        public async Task<bool> ProductExistsAsync(int productId)
        {
            return await _context.Products.AnyAsync(p => p.Id == productId);
        }
        public async Task AddAsync(ProductFeedback feedback)
        {
            await _context.ProductFeedbacks.AddAsync(feedback);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}