using Stylo.Backend.Stylo.Application.DTOs;

namespace Stylo.Backend.Stylo.Application.Interfaces
{
    public interface IProductService
    {
        Task<ProductListDto> GetAllAsync(
            int page,
            int pageSize,
            int? categoryId = null,
            string? gender = null);

        Task<ProductDto> GetByIdAsync(int id);

        Task<ProductDto> CreateAsync(CreateProductDto dto);

        Task<ProductDto> UpdateAsync(int id, UpdateProductDto dto);

        Task DeleteAsync(int id);
    }
}
