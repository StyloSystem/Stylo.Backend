using Stylo.Backend.Stylo.Domain.Entities;

namespace Stylo.Backend.Stylo.Application.Interfaces
{
    public interface IFavoriteRepository
    {
        Task<IEnumerable<Favorite>> GetByUserIdAsync(int userId);
        Task<Favorite?> GetByUserAndProductAsync(int userId, int productId);
        Task AddAsync(Favorite favorite);
        void Delete(Favorite favorite);
        Task SaveChangesAsync();
    }
}
