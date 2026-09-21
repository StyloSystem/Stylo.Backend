using Stylo.Backend.Stylo.Application.DTOs;
using Stylo.Backend.Stylo.Application.Exceptions;
using Stylo.Backend.Stylo.Application.Interfaces;
using Stylo.Backend.Stylo.Domain.Entities;

namespace Stylo.Backend.Stylo.Application.Services
{
    public class WebsiteFeedbackService : IWebsiteFeedbackService
    {
        private readonly IWebsiteFeedbackRepository _repository;

        public WebsiteFeedbackService(IWebsiteFeedbackRepository repository)
        {
            _repository = repository;
        }
        public async Task AddFeedbackAsync(int userId, CreateWebsiteFeedbackDto dto)
        {
            var trimmedMessage = dto.Message.Trim();

            if (trimmedMessage.Length < 5)
                throw new BadRequestException("Message must be at least 5 characters");

            if (trimmedMessage.Length > 1000)
                throw new BadRequestException("Message cannot exceed 1000 characters");

            if (trimmedMessage.Contains('<') || trimmedMessage.Contains('>'))
                throw new BadRequestException("Message cannot contain HTML tags");
            

            var feedback = new WebsiteFeedback
            {
                UserId = userId,
                Message = trimmedMessage,
                IsFeatured = false,
                CreatedAt = DateTime.UtcNow
            };

            await _repository.AddAsync(feedback);
            await _repository.SaveChangesAsync();
        }

        public async Task<IEnumerable<WebsiteFeedbackAdminDto>> GetAllAsync()
        {
            var feedbacks = await _repository.GetAllAsync();

            return feedbacks.Select(f => new WebsiteFeedbackAdminDto
            {
                Id = f.Id,
                Message = f.Message,
                IsFeatured = f.IsFeatured,
                CreatedAt = f.CreatedAt,
                UserId = f.UserId,
                UserName = f.User?.Name ?? "Unknown"
            });
        }

        public async Task<IEnumerable<WebsiteFeedbackDto>> GetFeaturedAsync()
        {
            var feedbacks = await _repository.GetFeaturedAsync();

            return feedbacks.Select(f => new WebsiteFeedbackDto
            {
                Id = f.Id,
                Message = f.Message,
                UserName = f.User?.Name ?? "Anonymous",
                CreatedAt = f.CreatedAt
            });
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

        public async Task UnfeatureAsync(int id)
        {
            if (id <= 0)
                throw new BadRequestException("Invalid feedback id");

            var feedback = await _repository.GetByIdAsync(id);

            if (feedback == null)
                throw new NotFoundException("Feedback not found");

            feedback.IsFeatured = false;

            await _repository.SaveChangesAsync();
        }
    }
}
