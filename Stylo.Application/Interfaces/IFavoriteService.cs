using Stylo.Backend.Stylo.Application.DTOs;

namespace Stylo.Backend.Stylo.Application.Interfaces
{
    public interface IFavoriteService
    {
        Task<IEnumerable<FavoriteDto>> GetUserFavoritesAsync(int userId);
        Task AddFavoriteAsync(int userId, CreateFavoriteDto dto);
        Task RemoveFavoriteAsync(int userId, int productId);
    }
}