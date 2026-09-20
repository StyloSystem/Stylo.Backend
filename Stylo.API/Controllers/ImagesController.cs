using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stylo.Backend.Stylo.Application.DTOs;
using Stylo.Backend.Stylo.Application.Interfaces;

namespace Stylo.Backend.Stylo.API.Controllers
{
    [ApiController]
    [Route("api/images")]
    public class ImagesController : ControllerBase
    {
        private readonly IImageService _imageService;

        public ImagesController(IImageService imageService)
        {
            _imageService = imageService;
        }

        [HttpPost("upload")]
        [Authorize(Roles = "Admin")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ImageUploadResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> UploadImage(IFormFile image)
        {
            var result = await _imageService.UploadImageAsync(image);

            return Ok(new ImageUploadResponseDto
            {
                ImageUrl = result.SecureUrl,
                ImagePublicId = result.PublicId
            });
        }
    }
}
