using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stylo.Backend.Stylo.Application.DTOs;
using Stylo.Backend.Stylo.Application.Interfaces;

namespace Stylo.Backend.Stylo.API.Controllers
{
    [ApiController]
    [Route("api/categories")]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(typeof(List<CategoryDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var categories = await _categoryService.GetAllAsync();
            return Ok(categories);
        }

        [HttpGet("{id:int}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var category = await _categoryService.GetByIdAsync(id);
            return Ok(category);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Create([FromBody] CreateCategoryDto dto)
        {
            var category = await _categoryService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = category.Id }, category);
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateCategoryDto dto)
        {
            var category = await _categoryService.UpdateAsync(id, dto);
            return Ok(category);
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Delete(int id)
        {
            await _categoryService.DeleteAsync(id);
            return NoContent();
        }
    }
}
