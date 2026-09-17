using Stylo.Backend.Stylo.Application.DTOs;
using Stylo.Backend.Stylo.Application.Exceptions;
using Stylo.Backend.Stylo.Application.Interfaces;
using Stylo.Backend.Stylo.Domain.Entities;

namespace Stylo.Backend.Stylo.Application.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;

        public CategoryService(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task<List<CategoryDto>> GetAllAsync()
        {
            var categories = await _categoryRepository.GetAllAsync();
            return categories.Select(ToDto).ToList();
        }

        public async Task<CategoryDto> GetByIdAsync(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
            {
                throw new NotFoundException("Category not found.", "CATEGORY_NOT_FOUND");
            }

            return ToDto(category);
        }

        public async Task<CategoryDto> CreateAsync(CreateCategoryDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Name))
            {
                throw new BadRequestException("Category name is required.");
            }

            var name = dto.Name.Trim();

            if (await _categoryRepository.NameExistsAsync(name))
            {
                throw new ConflictException("A category with this name already exists.", "CATEGORY_NAME_EXISTS");
            }

            var category = new Category { Name = name };
            await _categoryRepository.AddAsync(category);

            return ToDto(category);
        }

        public async Task<CategoryDto> UpdateAsync(int id, UpdateCategoryDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Name))
            {
                throw new BadRequestException("Category name is required.");
            }

            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
            {
                throw new NotFoundException("Category not found.", "CATEGORY_NOT_FOUND");
            }

            var name = dto.Name.Trim();

            if (await _categoryRepository.NameExistsAsync(name, excludeId: id))
            {
                throw new ConflictException("A category with this name already exists.", "CATEGORY_NAME_EXISTS");
            }

            category.Name = name;
            await _categoryRepository.UpdateAsync(category);

            return ToDto(category);
        }

        public async Task DeleteAsync(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
            {
                throw new NotFoundException("Category not found.", "CATEGORY_NOT_FOUND");
            }

            if (await _categoryRepository.HasProductsAsync(id))
            {
                throw new ConflictException(
                    "Cannot delete a category that has products assigned to it.",
                    "CATEGORY_IN_USE");
            }

            await _categoryRepository.DeleteAsync(category);
        }

        private static CategoryDto ToDto(Category category)
        {
            return new CategoryDto
            {
                Id = category.Id,
                Name = category.Name
            };
        }
    }
}
