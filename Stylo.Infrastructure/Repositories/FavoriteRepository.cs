using Microsoft.EntityFrameworkCore;
using Stylo.Backend.Stylo.Application.Interfaces;
using Stylo.Backend.Stylo.Domain.Entities;
using Stylo.Backend.Stylo.Infrastructure.Data;

namespace Stylo.Backend.Stylo.Infrastructure.Repositories
{
    public class FavoriteRepository : IFavoriteRepository
    {
        private readonly AppDbContext _context;
        public FavoriteRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Favorite>> GetByUserIdAsync(int userId)
            => await _context.Favorites
                .Include(f => f.Product)
                .Where(f => f.UserId == userId)
                .ToListAsync();
        public async Task<Favorite?> GetByUserAndProductAsync(int userId, int productId)
            => await _context.Favorites
                .Include(f => f.Product)
                .FirstOrDefaultAsync(f => f.UserId == userId && f.ProductId == productId);

        public async Task AddAsync(Favorite favorite)
            => await _context.Favorites.AddAsync(favorite);

        public void Delete(Favorite favorite)
            => _context.Favorites.Remove(favorite);

        public async Task SaveChangesAsync()
            => await _context.SaveChangesAsync();
    }
}
