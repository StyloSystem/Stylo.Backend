using Microsoft.AspNetCore.Http;
using Stylo.Backend.Stylo.Application.DTOs;

namespace Stylo.Backend.Stylo.Application.Interfaces
{
    public interface IImageService
    {
        Task<ImageUploadResult> UploadImageAsync(IFormFile file);
        Task DeleteImageAsync(string publicId);
    }
}
