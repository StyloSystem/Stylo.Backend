using Stylo.Backend.Stylo.Application.DTOs;
using Stylo.Backend.Stylo.Application.Exceptions;
using Stylo.Backend.Stylo.Application.Interfaces;
using Stylo.Backend.Stylo.Domain.Entities;

namespace Stylo.Backend.Stylo.Application.Services
{
    public class ProductFeedbackService : IProductFeedbackService
    {
        private readonly IProductFeedbackRepository _repository;

        public ProductFeedbackService(IProductFeedbackRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<ProductFeedbackDto>> GetByProductIdAsync(int productId)
        {
            var feedbacks = await _repository.GetByProductIdAsync(productId);

            return feedbacks.Select(f => new ProductFeedbackDto
            {
                Id = f.Id,
                Message = f.Message,
                UserName = f.User?.Name ?? "Anonymous",
                CreatedAt = f.CreatedAt
            });
        }

        public async Task<IEnumerable<ProductFeedbackAdminDto>> GetAllAsync()
        {
            var feedbacks = await _repository.GetAllAsync();

            return feedbacks.Select(f => new ProductFeedbackAdminDto
            {
                Id = f.Id,
                Message = f.Message,
                IsFeatured = f.IsFeatured,
                CreatedAt = f.CreatedAt,
                UserId = f.UserId,
                UserName = f.User?.Name ?? "Unknown",
                ProductId = f.ProductId,
                ProductName = f.Product?.Name ?? string.Empty
            });
        }

        public async Task<IEnumerable<FeaturedProductFeedbackDto>> GetFeaturedAsync()
        {
            var feedbacks = await _repository.GetFeaturedAsync();

            return feedbacks.Select(f => new FeaturedProductFeedbackDto
            {
                Id = f.Id,
                Message = f.Message,
                UserName = f.User?.Name ?? "Anonymous",
                ProductId = f.ProductId,
                ProductName = f.Product?.Name ?? string.Empty,
                ProductImageUrl = f.Product?.ImageUrl ?? string.Empty,
                CreatedAt = f.CreatedAt
            });
        }

        public async Task AddFeedbackAsync(int userId, int productId, CreateProductFeedbackDto dto)
        {
            var productExists = await _repository.ProductExistsAsync(productId);
            if (!productExists)
                throw new NotFoundException("Product not found");

            var trimmedMessage = dto.Message.Trim();

            if (trimmedMessage.Length < 5)
                throw new BadRequestException("Message must be at least 5 characters");

            if (trimmedMessage.Length > 1000)
                throw new BadRequestException("Message cannot exceed 1000 characters");

            if (trimmedMessage.Contains('<') || trimmedMessage.Contains('>'))
                throw new BadRequestException("Message cannot contain HTML tags");

            var orderId = await _repository.GetUserOrderIdForProductAsync(userId, productId);
            if (orderId == null)
                throw new BadRequestException("You can only review products you have purchased.");

            var hasPurchased = await _repository.HasUserPurchasedProductAsync(userId, productId);
            if (!hasPurchased)
                throw new BadRequestException("You can only review products you have purchased.");

            var alreadyReviewed = await _repository.HasUserAlreadyReviewedAsync(userId, productId);
            if (alreadyReviewed)
                throw new BadRequestException("You have already reviewed this product.");

            var feedback = new ProductFeedback
            {
                UserId = userId,
                ProductId = productId,
                OrderId = orderId.Value,
                Message = trimmedMessage,
                IsFeatured = false,
                CreatedAt = DateTime.UtcNow
            };

            await _repository.AddAsync(feedback);
            await _repository.SaveChangesAsync();
        }

        public async Task MarkAsFeaturedAsync(int id)
        {
            if (id <= 0)
                throw new BadRequestException("Invalid feedback id");

            var feedback = await _repository.GetByIdAsync(id);

            if (feedback == null)
                throw new NotFoundException("Feedback not found");

            feedback.IsFeatured = true;
            await _repository.SaveChangesAsync();
        }
    }
}