using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stylo.Backend.Stylo.Application.DTOs;
using Stylo.Backend.Stylo.Application.Interfaces;

namespace Stylo.Backend.Stylo.API.Controllers
{
    [ApiController]
    [Route("api/products")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

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

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [Consumes("application/json")]
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
            [FromBody] CreateProductDto dto)
        {
            var product = await _productService.CreateAsync(dto);

            return CreatedAtAction(
                nameof(GetById),
                new { id = product.Id },
                product);
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        [Consumes("application/json")]
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
            [FromBody] UpdateProductDto dto)
        {
            var product = await _productService.UpdateAsync(id, dto);

            return Ok(product);
        }

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
