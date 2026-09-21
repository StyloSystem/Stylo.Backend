using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stylo.Backend.Stylo.Application.DTOs;
using Stylo.Backend.Stylo.Application.Interfaces;

namespace Stylo.Backend.Stylo.API.Controllers
{
    /// <summary>
    /// Provides endpoints for browsing, creating, updating, and deleting product categories.
    /// </summary>
    [ApiController]
    [Route("api/categories")]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        /// <summary>
        /// Initializes a new instance of the <see cref="CategoriesController"/> class.
        /// </summary>
        /// <param name="categoryService">The service used to handle product category operations.</param>
        public CategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        /// <summary>
        /// Returns all available product categories.
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(typeof(List<CategoryDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var categories = await _categoryService.GetAllAsync();
            return Ok(categories);
        }

        /// <summary>
        /// Returns a specific category by its ID.
        /// </summary>
        [HttpGet("{id:int}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var category = await _categoryService.GetByIdAsync(id);
            return Ok(category);
        }

        /// <summary>
        /// Creates a new product category. Admin access is required.
        /// </summary>
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

        /// <summary>
        /// Updates an existing product category. Admin access is required.
        /// </summary>
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

        /// <summary>
        /// Deletes an existing product category. Admin access is required.
        /// </summary>
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
