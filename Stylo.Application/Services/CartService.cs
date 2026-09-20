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
            if (string.IsNullOrWhiteSpace(dto.Size) ||
                !Enum.TryParse<Stylo.Domain.Enums.Size>(dto.Size.Trim(), ignoreCase: true, out var parsedSize))
            {
                throw new BadRequestException($"Invalid product size '{dto.Size}'. Allowed sizes are: S, M, L, XL, XXL.", "INVALID_SIZE");
            }

            var product = await _productRepository.GetByIdAsync(dto.ProductId);
            if (product == null || product.IsDeleted)
            {
                throw new NotFoundException($"Product with ID {dto.ProductId} not found.");
            }

            var productSize = product.ProductSizes.FirstOrDefault(ps => ps.Size == parsedSize);
            if (productSize == null)
            {
                throw new BadRequestException($"Requested size '{dto.Size}' is not available for product '{product.Name}'.", "INVALID_SIZE");
            }

            var cart = await _cartRepository.GetOrCreateCartByUserIdAsync(userId);
            await _cartRepository.AddCartItemAsync(cart.Id, dto.ProductId, parsedSize.ToString(), dto.Quantity);

            var updatedCart = await _cartRepository.GetOrCreateCartByUserIdAsync(userId);
            return MapToDto(updatedCart);
        }

        public async Task<CartDto> UpdateCartItemAsync(int userId, int cartItemId, UpdateCartItemRequestDto dto)
        {
            var cartItem = await _cartRepository.GetCartItemByIdAsync(cartItemId);
            if (cartItem == null || cartItem.Cart.UserId != userId)
            {
                throw new NotFoundException($"Cart item with ID {cartItemId} not found for user.");
            }

            var product = await _productRepository.GetByIdAsync(cartItem.ProductId);
            if (product == null || product.IsDeleted)
            {
                throw new NotFoundException($"Product with ID {cartItem.ProductId} not found.");
            }

            if (!string.IsNullOrWhiteSpace(dto.Size))
            {
                if (!Enum.TryParse<Stylo.Domain.Enums.Size>(dto.Size.Trim(), ignoreCase: true, out var parsedSize))
                {
                    throw new BadRequestException($"Invalid product size '{dto.Size}'. Allowed sizes are: S, M, L, XL, XXL.", "INVALID_SIZE");
                }

                var productSize = product.ProductSizes.FirstOrDefault(ps => ps.Size == parsedSize);
                if (productSize == null)
                {
                    throw new BadRequestException($"Requested size '{dto.Size}' is not available for product '{product.Name}'.", "INVALID_SIZE");
                }

                cartItem.Size = parsedSize.ToString();
            }

            if (dto.Quantity.HasValue)
            {
                if (dto.Quantity.Value <= 0)
                {
                    await _cartRepository.RemoveCartItemAsync(cartItem);
                    var cartAfterRemove = await _cartRepository.GetOrCreateCartByUserIdAsync(userId);
                    return MapToDto(cartAfterRemove);
                }
                cartItem.Quantity = dto.Quantity.Value;
            }

            await _cartRepository.UpdateCartItemAsync(cartItem);

            var updatedCart = await _cartRepository.GetOrCreateCartByUserIdAsync(userId);
            return MapToDto(updatedCart);
        }

        public async Task<CartDto> UpdateCartItemQuantityAsync(int userId, int cartItemId, int quantity)
        {
            return await UpdateCartItemAsync(userId, cartItemId, new UpdateCartItemRequestDto { Quantity = quantity });
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
            var validCartItems = cart.CartItems
                .Where(ci => ci.Product != null && !ci.Product.IsDeleted)
                .ToList();

            var items = validCartItems.Select(ci => new CartItemDto
            {
                Id = ci.Id,
                ProductId = ci.ProductId,
                ProductName = ci.Product!.Name,
                ProductImageUrl = ci.Product.ImageUrl ?? string.Empty,
                Size = ci.Size,
                Quantity = ci.Quantity,
                UnitPrice = ci.Product.Price,
                TotalPrice = ci.Product.Price * ci.Quantity
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
