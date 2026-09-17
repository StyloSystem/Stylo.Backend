using Stylo.Backend.Stylo.Domain.Entities;

namespace Stylo.Backend.Stylo.Application.Interfaces
{
    public interface ICartRepository
    {
        Task<Cart> GetOrCreateCartByUserIdAsync(int userId);
        Task<CartItem?> GetCartItemByIdAsync(int cartItemId);
        Task<CartItem> AddCartItemAsync(int cartId, int productId, string size, int quantity);
        Task UpdateCartItemAsync(CartItem cartItem);
        Task RemoveCartItemAsync(CartItem cartItem);
        Task ClearCartAsync(int cartId);
    }
}
