using Stylo.Backend.Stylo.Application.DTOs;
using Stylo.Backend.Stylo.Application.Exceptions;
using Stylo.Backend.Stylo.Application.Interfaces;
using Stylo.Backend.Stylo.Domain.Entities;
using Stylo.Backend.Stylo.Infrastructure.Repositories;

namespace Stylo.Backend.Stylo.Application.Services
{
    public class FavoriteService : IFavoriteService
    {
        private readonly IFavoriteRepository _repository;

        public FavoriteService(IFavoriteRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<FavoriteDto>> GetUserFavoritesAsync(int userId)
        {
            var favourites = await _repository.GetByUserIdAsync(userId);

            return favourites.Select(f => new FavoriteDto
            {
                Id = f.Id,
                ProductId = f.ProductId,
                ProductName = f.Product?.Name ?? string.Empty,
                ProductImageUrl = f.Product?.ImageUrl ?? string.Empty,
                ProductPrice = f.Product?.Price ?? 0
            });
        }

        public async Task AddFavoriteAsync(int userId, CreateFavoriteDto dto)
        {
            var existing = await _repository.GetByUserAndProductAsync(userId, dto.ProductId);

            if (existing != null)
                throw new BadRequestException("Product is already in favourites");

            var favourite = new Favorite
            {
                UserId = userId,
                ProductId = dto.ProductId
            };

            await _repository.AddAsync(favourite);
            await _repository.SaveChangesAsync();
        }

        public async Task RemoveFavoriteAsync(int userId, int productId)
        {
            if (productId <= 0)
                throw new BadRequestException("Invalid product id");

            var favourite = await _repository.GetByUserAndProductAsync(userId, productId);

            if (favourite == null)
                throw new NotFoundException("Favourite not found");

            _repository.Delete(favourite);
            await _repository.SaveChangesAsync();
        }
    }
}