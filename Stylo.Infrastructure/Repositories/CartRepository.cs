using Microsoft.EntityFrameworkCore;
using Stylo.Backend.Stylo.Application.Interfaces;
using Stylo.Backend.Stylo.Domain.Entities;
using Stylo.Backend.Stylo.Infrastructure.Data;

namespace Stylo.Backend.Stylo.Infrastructure.Repositories
{
    public class CartRepository : ICartRepository
    {
        private readonly AppDbContext _context;

        public CartRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Cart> GetOrCreateCartByUserIdAsync(int userId)
        {
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new Cart
                {
                    UserId = userId
                };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();

                cart = await _context.Carts
                    .Include(c => c.CartItems)
                        .ThenInclude(ci => ci.Product)
                    .FirstAsync(c => c.Id == cart.Id);
            }
            else
            {
                var invalidItems = cart.CartItems
                    .Where(ci => ci.Product == null || ci.Product.IsDeleted)
                    .ToList();

                if (invalidItems.Count > 0)
                {
                    _context.CartItems.RemoveRange(invalidItems);
                    await _context.SaveChangesAsync();

                    foreach (var item in invalidItems)
                    {
                        cart.CartItems.Remove(item);
                    }
                }
            }

            return cart;
        }

        public async Task<CartItem?> GetCartItemByIdAsync(int cartItemId)
        {
            return await _context.CartItems
                .Include(ci => ci.Cart)
                .Include(ci => ci.Product)
                .FirstOrDefaultAsync(ci => ci.Id == cartItemId);
        }

        public async Task<CartItem> AddCartItemAsync(int cartId, int productId, string size, int quantity)
        {
            var existingItem = await _context.CartItems
                .FirstOrDefaultAsync(ci => ci.CartId == cartId && ci.ProductId == productId && ci.Size == size);

            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
                _context.CartItems.Update(existingItem);
                await _context.SaveChangesAsync();
                return existingItem;
            }

            var newItem = new CartItem
            {
                CartId = cartId,
                ProductId = productId,
                Size = size,
                Quantity = quantity
            };
            _context.CartItems.Add(newItem);
            await _context.SaveChangesAsync();
            return newItem;
        }

        public async Task UpdateCartItemAsync(CartItem cartItem)
        {
            _context.CartItems.Update(cartItem);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveCartItemAsync(CartItem cartItem)
        {
            _context.CartItems.Remove(cartItem);
            await _context.SaveChangesAsync();
        }

        public async Task ClearCartAsync(int cartId)
        {
            var items = await _context.CartItems
                .Where(ci => ci.CartId == cartId)
                .ToListAsync();

            if (items.Count > 0)
            {
                _context.CartItems.RemoveRange(items);
                await _context.SaveChangesAsync();
            }
        }
    }
}
