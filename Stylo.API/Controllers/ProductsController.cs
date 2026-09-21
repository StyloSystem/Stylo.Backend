using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stylo.Backend.Stylo.Application.DTOs;
using Stylo.Backend.Stylo.Application.Interfaces;

namespace Stylo.Backend.Stylo.API.Controllers
{
    /// <summary>
    /// Provides endpoints for browsing, creating, updating, and deleting products.
    /// </summary>
    [ApiController]
    [Route("api/products")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProductsController"/> class.
        /// </summary>
        /// <param name="productService">The service used to handle product inventory operations.</param>
        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        /// <summary>
        /// Returns a paginated list of products.
        /// </summary>
        /// <param name="page">The page number. Starts from 1.</param>
        /// <param name="pageSize">The number of products returned per page.</param>
        /// <param name="categoryId">Optional category filter.</param>
        /// <param name="gender">Optional gender filter.</param>
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(
            typeof(ProductListDto),
            StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 12,
            [FromQuery] int? categoryId = null,
            [FromQuery] string? gender = null)
        {
            var products = await _productService.GetAllAsync(
                page,
                pageSize,
                categoryId,
                gender);

            return Ok(products);
        }

        /// <summary>
        /// Returns a product by its ID.
        /// </summary>
        [HttpGet("{id:int}")]
        [AllowAnonymous]
        [ProducesResponseType(
            typeof(ProductDto),
            StatusCodes.Status200OK)]
        [ProducesResponseType(
            typeof(ErrorResponseDto),
            StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var product = await _productService.GetByIdAsync(id);

            return Ok(product);
        }

        /// <summary>
        /// Creates a new product with an image. Admin access is required.
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(
            typeof(ProductDto),
            StatusCodes.Status201Created)]
        [ProducesResponseType(
            typeof(ErrorResponseDto),
            StatusCodes.Status400BadRequest)]
        [ProducesResponseType(
            typeof(ErrorResponseDto),
            StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(
            typeof(ErrorResponseDto),
            StatusCodes.Status403Forbidden)]
        [ProducesResponseType(
            typeof(ErrorResponseDto),
            StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Create(
            [FromForm] CreateProductDto dto)
        {
            var product = await _productService.CreateAsync(dto);

            return CreatedAtAction(
                nameof(GetById),
                new { id = product.Id },
                product);
        }

        /// <summary>
        /// Updates an existing product. Admin access is required.
        /// </summary>
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(
            typeof(ProductDto),
            StatusCodes.Status200OK)]
        [ProducesResponseType(
            typeof(ErrorResponseDto),
            StatusCodes.Status400BadRequest)]
        [ProducesResponseType(
            typeof(ErrorResponseDto),
            StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(
            typeof(ErrorResponseDto),
            StatusCodes.Status403Forbidden)]
        [ProducesResponseType(
            typeof(ErrorResponseDto),
            StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(
            int id,
            [FromForm] UpdateProductDto dto)
        {
            var product = await _productService.UpdateAsync(id, dto);

            return Ok(product);
        }

        /// <summary>
        /// Deletes a product. Admin access is required.
        /// </summary>
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(
            typeof(ErrorResponseDto),
            StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(
            typeof(ErrorResponseDto),
            StatusCodes.Status403Forbidden)]
        [ProducesResponseType(
            typeof(ErrorResponseDto),
            StatusCodes.Status404NotFound)]
        [ProducesResponseType(
            typeof(ErrorResponseDto),
            StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Delete(int id)
        {
            await _productService.DeleteAsync(id);

            return NoContent();
        }
    }
}
