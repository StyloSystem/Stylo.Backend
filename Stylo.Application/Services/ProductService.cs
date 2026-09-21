using Stylo.Backend.Stylo.Application.DTOs;
using Stylo.Backend.Stylo.Application.Exceptions;
using Stylo.Backend.Stylo.Application.Interfaces;
using Stylo.Backend.Stylo.Domain.Entities;
using Stylo.Backend.Stylo.Domain.Enums;
using System.Security.Claims;

namespace Stylo.Backend.Stylo.Application.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly IImageService _imageService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ProductService(
            IProductRepository productRepository,
            IImageService imageService,
            IHttpContextAccessor httpContextAccessor)
        {
            _productRepository = productRepository;
            _imageService = imageService;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<ProductListDto> GetAllAsync(
            int page,
            int pageSize,
            int? categoryId = null,
            string? gender = null)
        {
            if (page < 1)
                page = 1;

            if (pageSize < 1)
                pageSize = 12;

            var products = await _productRepository.GetAllAsync(
                page,
                pageSize,
                categoryId,
                gender);

            var totalCount = await _productRepository.GetCountAsync(
                categoryId,
                gender);

            var items = new List<ProductDto>();

            foreach (var product in products)
            {
                items.Add(await ToDto(product));
            }

            return new ProductListDto
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<ProductDto> GetByIdAsync(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);

            if (product == null)
            {
                throw new NotFoundException(
                    "Product not found.",
                    "PRODUCT_NOT_FOUND");
            }

            return await ToDto(product);
        }

        public async Task<ProductDto> CreateAsync(CreateProductDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Name))
            {
                throw new BadRequestException("Product name is required.");
            }

            if (string.IsNullOrWhiteSpace(dto.Gender))
            {
                throw new BadRequestException("Gender is required.");
            }

            if (!Enum.TryParse<Gender>(
                    dto.Gender.Trim(),
                    true,
                    out var gender))
            {
                throw new BadRequestException(
                    "Invalid gender.",
                    "INVALID_GENDER");
            }

            if (!await _productRepository.CategoryExistsAsync(dto.CategoryId))
            {
                throw new NotFoundException(
                    "Category not found.",
                    "CATEGORY_NOT_FOUND");
            }

            string? imageUrl =
                string.IsNullOrWhiteSpace(dto.ImageUrl)
                    ? null
                    : dto.ImageUrl.Trim();

            string? imagePublicId =
                string.IsNullOrWhiteSpace(dto.ImagePublicId)
                    ? null
                    : dto.ImagePublicId.Trim();

            if (dto.Image != null && dto.Image.Length > 0)
            {
                var uploadResult =
                    await _imageService.UploadImageAsync(dto.Image);

                imageUrl = uploadResult.SecureUrl;
                imagePublicId = uploadResult.PublicId;
            }

            var product = new Product
            {
                Name = dto.Name.Trim(),

                Description = string.IsNullOrWhiteSpace(dto.Description)
                    ? null
                    : dto.Description.Trim(),

                Price = dto.Price,

                ImageUrl = imageUrl,
                ImagePublicId = imagePublicId,

                Gender = gender,
                CategoryId = dto.CategoryId
            };

            AddProductSizes(product, dto.Sizes);

            try
            {
                await _productRepository.AddAsync(product);
            }
            catch
            {
                if (!string.IsNullOrWhiteSpace(imagePublicId))
                {
                    await _imageService.DeleteImageAsync(imagePublicId);
                }

                throw;
            }

            var createdProduct =
                await _productRepository.GetByIdAsync(product.Id);

            if (createdProduct == null)
            {
                if (!string.IsNullOrWhiteSpace(imagePublicId))
                {
                    await _imageService.DeleteImageAsync(imagePublicId);
                }

                throw new NotFoundException(
                    "Product could not be retrieved after creation.",
                    "PRODUCT_NOT_FOUND");
            }

            return await ToDto(createdProduct);
        }

        public async Task<ProductDto> UpdateAsync(
            int id,
            UpdateProductDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Name))
            {
                throw new BadRequestException(
                    "Product name is required.");
            }

            if (string.IsNullOrWhiteSpace(dto.Gender))
            {
                throw new BadRequestException(
                    "Gender is required.");
            }

            var product =
                await _productRepository.GetByIdAsync(id);

            if (product == null || product.IsDeleted)
            {
                throw new NotFoundException(
                    "Product not found.",
                    "PRODUCT_NOT_FOUND");
            }

            if (!Enum.TryParse<Gender>(
                    dto.Gender.Trim(),
                    true,
                    out var gender))
            {
                throw new BadRequestException(
                    "Invalid gender.",
                    "INVALID_GENDER");
            }

            if (!await _productRepository.CategoryExistsAsync(
                    dto.CategoryId))
            {
                throw new NotFoundException(
                    "Category not found.",
                    "CATEGORY_NOT_FOUND");
            }

            product.Name = dto.Name.Trim();

            product.Description =
                string.IsNullOrWhiteSpace(dto.Description)
                    ? null
                    : dto.Description.Trim();

            product.Price = dto.Price;
            product.Gender = gender;
            product.CategoryId = dto.CategoryId;

            string? newPublicIdToCleanupOnFailure = null;
            string? oldPublicIdToDelete = null;

            if (dto.Image != null && dto.Image.Length > 0)
            {
                var uploadResult =
                    await _imageService.UploadImageAsync(dto.Image);

                newPublicIdToCleanupOnFailure = uploadResult.PublicId;
                oldPublicIdToDelete = product.ImagePublicId;

                product.ImageUrl = uploadResult.SecureUrl;
                product.ImagePublicId = uploadResult.PublicId;
            }
            else if (!string.IsNullOrWhiteSpace(dto.ImagePublicId) &&
                dto.ImagePublicId != product.ImagePublicId)
            {
                newPublicIdToCleanupOnFailure = dto.ImagePublicId;
                oldPublicIdToDelete = product.ImagePublicId;

                product.ImageUrl =
                    string.IsNullOrWhiteSpace(dto.ImageUrl)
                        ? null
                        : dto.ImageUrl.Trim();

                product.ImagePublicId =
                    dto.ImagePublicId.Trim();
            }
            else if (!string.IsNullOrWhiteSpace(dto.ImageUrl) &&
                     string.IsNullOrWhiteSpace(dto.ImagePublicId))
            {
                product.ImageUrl = dto.ImageUrl.Trim();
            }

            UpdateProductSizes(product, dto.Sizes);

            try
            {
                await _productRepository.UpdateAsync(product);
            }
            catch
            {
                if (!string.IsNullOrWhiteSpace(
                        newPublicIdToCleanupOnFailure))
                {
                    await _imageService.DeleteImageAsync(
                        newPublicIdToCleanupOnFailure);
                }

                throw;
            }

            if (!string.IsNullOrWhiteSpace(oldPublicIdToDelete) &&
                oldPublicIdToDelete != product.ImagePublicId)
            {
                try
                {
                    await _imageService.DeleteImageAsync(
                        oldPublicIdToDelete);
                }
                catch
                {
                }
            }

            var updatedProduct =
                await _productRepository.GetByIdAsync(id);

            if (updatedProduct == null)
            {
                throw new NotFoundException(
                    "Product not found.",
                    "PRODUCT_NOT_FOUND");
            }

            return await ToDto(updatedProduct);
        }

        public async Task DeleteAsync(int id)
        {
            var product =
                await _productRepository.GetByIdAsync(id);

            if (product == null)
            {
                throw new NotFoundException(
                    "Product not found.",
                    "PRODUCT_NOT_FOUND");
            }

            await _productRepository.DeleteAsync(product);
        }

        private static void AddProductSizes(
            Product product,
            List<CreateProductSizeDto> sizeDtos)
        {
            if (sizeDtos == null || !sizeDtos.Any())
            {
                throw new BadRequestException(
                    "At least one product size and stock is required.",
                    "INVALID_SIZE");
            }

            var existingSizes = new HashSet<Size>();

            foreach (var sizeDto in sizeDtos)
            {
                if (sizeDto == null ||
                    string.IsNullOrWhiteSpace(sizeDto.Size))
                {
                    throw new BadRequestException(
                        "Size is required.",
                        "INVALID_SIZE");
                }

                if (!Enum.TryParse<Size>(
                        sizeDto.Size.Trim(),
                        true,
                        out var size))
                {
                    throw new BadRequestException(
                        $"Invalid size: {sizeDto.Size}.",
                        "INVALID_SIZE");
                }

                if (!existingSizes.Add(size))
                {
                    throw new BadRequestException(
                        $"Duplicate size: {sizeDto.Size}.",
                        "DUPLICATE_SIZE");
                }

                product.ProductSizes.Add(new ProductSize
                {
                    Size = size,
                    Stock = sizeDto.Stock
                });
            }
        }

        private static void UpdateProductSizes(
            Product product,
            List<CreateProductSizeDto> sizeDtos)
        {
            if (sizeDtos != null && sizeDtos.Any())
            {
                var requestedSizes = new HashSet<Size>();

                foreach (var sizeDto in sizeDtos)
                {
                    if (sizeDto == null ||
                        string.IsNullOrWhiteSpace(sizeDto.Size))
                    {
                        throw new BadRequestException(
                            "Size is required.",
                            "INVALID_SIZE");
                    }

                    if (!Enum.TryParse<Size>(
                            sizeDto.Size.Trim(),
                            true,
                            out var size))
                    {
                        throw new BadRequestException(
                            $"Invalid size: {sizeDto.Size}.",
                            "INVALID_SIZE");
                    }

                    if (!requestedSizes.Add(size))
                    {
                        throw new BadRequestException(
                            $"Duplicate size: {sizeDto.Size}.",
                            "DUPLICATE_SIZE");
                    }

                    var existingSize = product.ProductSizes
                        .FirstOrDefault(ps => ps.Size == size);

                    if (existingSize != null)
                    {
                        existingSize.Stock = sizeDto.Stock;
                    }
                    else
                    {
                        product.ProductSizes.Add(new ProductSize
                        {
                            ProductId = product.Id,
                            Size = size,
                            Stock = sizeDto.Stock
                        });
                    }
                }
            }

            if (!product.ProductSizes.Any())
            {
                throw new BadRequestException(
                    "At least one product size and stock is required.",
                    "INVALID_SIZE");
            }
        }

        private int? GetCurrentUserId()
        {
            var userIdValue = _httpContextAccessor
                .HttpContext?
                .User?
                .FindFirstValue(ClaimTypes.NameIdentifier);

            if (int.TryParse(userIdValue, out var userId))
            {
                return userId;
            }

            return null;
        }

        private async Task<ProductDto> ToDto(Product product)
        {
            var firstSize =
                product.ProductSizes.FirstOrDefault();

            var hasPurchased =
                await _productRepository.HasPurchasedProductIdsAsync(
                    GetCurrentUserId(),
                    product.Id);

            return new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                ImageUrl = product.ImageUrl,
                ImagePublicId = product.ImagePublicId,
                Gender = product.Gender.ToString(),

                Size = firstSize?.Size.ToString(),
                Stock = firstSize?.Stock ?? 0,

                HasPurchased = hasPurchased,

                Category = new ProductCategoryDto
                {
                    Id = product.Category.Id,
                    Name = product.Category.Name
                },

                Sizes = product.ProductSizes
                    .Select(ps => new ProductSizeDto
                    {
                        Size = ps.Size.ToString(),
                        Stock = ps.Stock,
                        IsAvailable = ps.Stock > 0
                    })
                    .ToList(),

                CreatedAt = product.CreatedAt
            };
        }
    }
}