using System.IO;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Stylo.Backend.Stylo.Application.Exceptions;
using Stylo.Backend.Stylo.Application.Interfaces;
using Stylo.Backend.Stylo.Infrastructure.Configurations;
using ApplicationImageUploadResult = Stylo.Backend.Stylo.Application.DTOs.ImageUploadResult;

namespace Stylo.Backend.Stylo.Infrastructure.Services
{
    public class CloudinaryImageService : IImageService
    {
        private readonly IOptions<CloudinarySettings> _config;
        private static readonly HashSet<string> AllowedMimeTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "image/jpeg",
            "image/jpg",
            "image/png",
            "image/webp"
        };

        private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg",
            ".jpeg",
            ".png",
            ".webp"
        };

        private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5 MB

        public CloudinaryImageService(IOptions<CloudinarySettings> config)
        {
            _config = config;
        }

        private Cloudinary GetCloudinaryClient()
        {
            var settings = _config.Value;
            if (string.IsNullOrWhiteSpace(settings.CloudName) ||
                string.IsNullOrWhiteSpace(settings.ApiKey) ||
                string.IsNullOrWhiteSpace(settings.ApiSecret))
            {
                throw new BadRequestException("Cloudinary credentials are not configured.", "CLOUDINARY_NOT_CONFIGURED");
            }

            var account = new Account(
                settings.CloudName,
                settings.ApiKey,
                settings.ApiSecret);

            return new Cloudinary(account);
        }

        public async Task<ApplicationImageUploadResult> UploadImageAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                throw new BadRequestException("Image file is required.", "INVALID_IMAGE");
            }

            if (file.Length > MaxFileSizeBytes)
            {
                throw new BadRequestException("Image file size must not exceed 5 MB.", "FILE_TOO_LARGE");
            }

            var extension = Path.GetExtension(file.FileName);
            if (string.IsNullOrEmpty(extension) || !AllowedExtensions.Contains(extension) || !AllowedMimeTypes.Contains(file.ContentType))
            {
                throw new BadRequestException("Invalid image format. Only JPG, JPEG, PNG, and WEBP formats are allowed.", "INVALID_IMAGE_FORMAT");
            }

            var cloudinary = GetCloudinaryClient();

            using var stream = file.OpenReadStream();
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = "stylo/products"
            };

            var uploadResult = await cloudinary.UploadAsync(uploadParams);

            if (uploadResult.Error != null)
            {
                throw new BadRequestException($"Image upload failed: {uploadResult.Error.Message}", "IMAGE_UPLOAD_FAILED");
            }

            return new ApplicationImageUploadResult
            {
                SecureUrl = uploadResult.SecureUrl?.ToString() ?? uploadResult.Url?.ToString() ?? string.Empty,
                PublicId = uploadResult.PublicId
            };
        }

        public async Task DeleteImageAsync(string publicId)
        {
            if (string.IsNullOrWhiteSpace(publicId))
            {
                return;
            }

            var cloudinary = GetCloudinaryClient();

            var deleteParams = new DeletionParams(publicId);
            await cloudinary.DestroyAsync(deleteParams);
        }
    }
}
