using Stylo.Backend.Stylo.Application.DTOs;
using Stylo.Backend.Stylo.Application.Exceptions;
using Stylo.Backend.Stylo.Application.Interfaces;
using Stylo.Backend.Stylo.Domain.Entities;

namespace Stylo.Backend.Stylo.Application.Services
{
    public class CartService : ICartService
    {
        private readonly ICartRepository _cartRepository;
        private readonly IProductRepository _productRepository;

        public CartService(ICartRepository cartRepository, IProductRepository productRepository)
        {
            _cartRepository = cartRepository;
            _productRepository = productRepository;
        }

        public async Task<CartDto> GetUserCartAsync(int userId)
        {
            var cart = await _cartRepository.GetOrCreateCartByUserIdAsync(userId);
            return MapToDto(cart);
        }

        public async Task<CartDto> AddItemToCartAsync(int userId, AddToCartRequestDto dto)
        {
            var product = await _productRepository.GetByIdAsync(dto.ProductId);
            if (product == null)
            {
                throw new NotFoundException($"Product with ID {dto.ProductId} not found.");
            }

            var cart = await _cartRepository.GetOrCreateCartByUserIdAsync(userId);
            await _cartRepository.AddCartItemAsync(cart.Id, dto.ProductId, dto.Size, dto.Quantity);

            var updatedCart = await _cartRepository.GetOrCreateCartByUserIdAsync(userId);
            return MapToDto(updatedCart);
        }

        public async Task<CartDto> UpdateCartItemQuantityAsync(int userId, int cartItemId, int quantity)
        {
            var cartItem = await _cartRepository.GetCartItemByIdAsync(cartItemId);
            if (cartItem == null || cartItem.Cart.UserId != userId)
            {
                throw new NotFoundException($"Cart item with ID {cartItemId} not found for user.");
            }

            if (quantity <= 0)
            {
                await _cartRepository.RemoveCartItemAsync(cartItem);
            }
            else
            {
                cartItem.Quantity = quantity;
                await _cartRepository.UpdateCartItemAsync(cartItem);
            }

            var updatedCart = await _cartRepository.GetOrCreateCartByUserIdAsync(userId);
            return MapToDto(updatedCart);
        }

        public async Task<CartDto> RemoveCartItemAsync(int userId, int cartItemId)
        {
            var cartItem = await _cartRepository.GetCartItemByIdAsync(cartItemId);
            if (cartItem == null || cartItem.Cart.UserId != userId)
            {
                throw new NotFoundException($"Cart item with ID {cartItemId} not found for user.");
            }

            await _cartRepository.RemoveCartItemAsync(cartItem);

            var updatedCart = await _cartRepository.GetOrCreateCartByUserIdAsync(userId);
            return MapToDto(updatedCart);
        }

        public async Task ClearCartAsync(int userId)
        {
            var cart = await _cartRepository.GetOrCreateCartByUserIdAsync(userId);
            await _cartRepository.ClearCartAsync(cart.Id);
        }

        private static CartDto MapToDto(Cart cart)
        {
            var items = cart.CartItems.Select(ci => new CartItemDto
            {
                Id = ci.Id,
                ProductId = ci.ProductId,
                ProductName = ci.Product?.Name ?? string.Empty,
                ProductImageUrl = ci.Product?.ImageUrl ?? string.Empty,
                Size = ci.Size,
                Quantity = ci.Quantity,
                UnitPrice = ci.Product?.Price ?? 0,
                TotalPrice = (ci.Product?.Price ?? 0) * ci.Quantity
            }).ToList();

            return new CartDto
            {
                Id = cart.Id,
                UserId = cart.UserId,
                Items = items,
                GrandTotal = items.Sum(i => i.TotalPrice)
            };
        }
    }
}
