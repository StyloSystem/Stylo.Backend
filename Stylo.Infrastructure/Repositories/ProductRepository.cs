using Stylo.Backend.Stylo.Application.Interfaces;
using Stylo.Backend.Stylo.Domain.Entities;
using Stylo.Backend.Stylo.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
namespace Stylo.Backend.Stylo.Infrastructure.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly AppDbContext _context;

        public ProductRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Product>> GetAllAsync(
            int page,
            int pageSize,
            int? categoryId = null,
            string? gender = null)
        {
            var query = _context.Products
                .AsNoTracking()
                .Include(p => p.Category)
                .Include(p => p.ProductSizes)
                .AsQueryable();

            if (categoryId.HasValue)
            {
                query = query.Where(
                    p => p.CategoryId == categoryId.Value);
            }

            if (!string.IsNullOrWhiteSpace(gender))
            {
                query = query.Where(
                    p => p.Gender.ToString().ToLower() ==
                         gender.Trim().ToLower());
            }

            return await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetCountAsync(
            int? categoryId = null,
            string? gender = null)
        {
            var query = _context.Products.AsQueryable();

            if (categoryId.HasValue)
            {
                query = query.Where(
                    p => p.CategoryId == categoryId.Value);
            }

            if (!string.IsNullOrWhiteSpace(gender))
            {
                query = query.Where(
                    p => p.Gender.ToString().ToLower() ==
                         gender.Trim().ToLower());
            }

            return await query.CountAsync();
        }

        public async Task<Product?> GetByIdAsync(int id)
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductSizes)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<bool> CategoryExistsAsync(int categoryId)
        {
            return await _context.Categories
                .AnyAsync(c => c.Id == categoryId);
        }

        public async Task<bool> HasCartItemsAsync(int productId)
        {
            return await _context.CartItems
                .AnyAsync(ci => ci.ProductId == productId);
        }

        public async Task<bool> HasOrderItemsAsync(int productId)
        {
            return await _context.OrderItems
                .AnyAsync(oi => oi.ProductId == productId);
        }

        public async Task<bool> HasProductFeedbacksAsync(int productId)
        {
            return await _context.ProductFeedbacks
                .AnyAsync(pf => pf.ProductId == productId);
        }

        public async Task<bool> HasPurchasedProductIdsAsync(
            int? userId,
            int productId)
        {
            return await _context.OrderItems
                .AnyAsync(oi =>
                    oi.ProductId == productId &&
                    oi.Order.UserId == (userId ?? 0) &&
                    oi.Order.Status ==
                        Domain.Enums.OrderStatus.Confirmed);
        }
        public async Task AddAsync(Product product)
        {
            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Product product)
        {
            _context.Products.Update(product);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Product product)
        {
            product.IsDeleted = true;
            _context.Products.Update(product);

            var cartItems = await _context.CartItems
                .Where(ci => ci.ProductId == product.Id)
                .ToListAsync();

            if (cartItems.Count > 0)
            {
                _context.CartItems.RemoveRange(cartItems);
            }

            await _context.SaveChangesAsync();
        }
    }
}
