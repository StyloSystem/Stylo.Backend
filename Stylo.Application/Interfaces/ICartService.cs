using Stylo.Backend.Stylo.Application.DTOs;

namespace Stylo.Backend.Stylo.Application.Interfaces
{
    public interface ICartService
    {
        Task<CartDto> GetUserCartAsync(int userId);
        Task<CartDto> AddItemToCartAsync(int userId, AddToCartRequestDto dto);
        Task<CartDto> UpdateCartItemAsync(int userId, int cartItemId, UpdateCartItemRequestDto dto);
        Task<CartDto> UpdateCartItemQuantityAsync(int userId, int cartItemId, int quantity);
        Task<CartDto> RemoveCartItemAsync(int userId, int cartItemId);
        Task ClearCartAsync(int userId);
    }
}
