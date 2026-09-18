using Stylo.Backend.Stylo.Application.DTOs;
using Stylo.Backend.Stylo.Application.Exceptions;
using Stylo.Backend.Stylo.Application.Interfaces;
using Stylo.Backend.Stylo.Domain.Entities;
using Stylo.Backend.Stylo.Domain.Enums;

namespace Stylo.Backend.Stylo.Application.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;

        public ProductService(IProductRepository productRepository)
        {
            _productRepository = productRepository;
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

            return new ProductListDto
            {
                Items = products.Select(ToDto).ToList(),
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

            return ToDto(product);
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

            var product = new Product
            {
                Name = dto.Name.Trim(),
                Description = string.IsNullOrWhiteSpace(dto.Description)
                    ? null
                    : dto.Description.Trim(),
                Price = dto.Price,
                ImageUrl = string.IsNullOrWhiteSpace(dto.ImageUrl)
                    ? null
                    : dto.ImageUrl.Trim(),
                Gender = gender,
                CategoryId = dto.CategoryId
            };

            AddProductSizes(product, dto.Sizes);

            await _productRepository.AddAsync(product);

            var createdProduct = await _productRepository.GetByIdAsync(product.Id);

            if (createdProduct == null)
            {
                throw new NotFoundException(
                    "Product could not be retrieved after creation.",
                    "PRODUCT_NOT_FOUND");
            }

            return ToDto(createdProduct);
        }

        public async Task<ProductDto> UpdateAsync(
            int id,
            UpdateProductDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Name))
            {
                throw new BadRequestException("Product name is required.");
            }

            if (string.IsNullOrWhiteSpace(dto.Gender))
            {
                throw new BadRequestException("Gender is required.");
            }

            var product = await _productRepository.GetByIdAsync(id);

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

            if (!await _productRepository.CategoryExistsAsync(dto.CategoryId))
            {
                throw new NotFoundException(
                    "Category not found.",
                    "CATEGORY_NOT_FOUND");
            }

            product.Name = dto.Name.Trim();

            product.Description = string.IsNullOrWhiteSpace(dto.Description)
                ? null
                : dto.Description.Trim();

            product.Price = dto.Price;

            product.ImageUrl = string.IsNullOrWhiteSpace(dto.ImageUrl)
                ? null
                : dto.ImageUrl.Trim();

            product.Gender = gender;
            product.CategoryId = dto.CategoryId;

            UpdateProductSizes(product, dto.Sizes);

            await _productRepository.UpdateAsync(product);

            var updatedProduct = await _productRepository.GetByIdAsync(id);

            if (updatedProduct == null)
            {
                throw new NotFoundException(
                    "Product not found.",
                    "PRODUCT_NOT_FOUND");
            }

            return ToDto(updatedProduct);
        }

        public async Task DeleteAsync(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);

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

        private static ProductDto ToDto(Product product)
        {
            return new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                ImageUrl = product.ImageUrl,
                Gender = product.Gender.ToString(),

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
