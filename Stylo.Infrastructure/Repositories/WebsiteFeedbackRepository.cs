using Microsoft.EntityFrameworkCore;
using Stylo.Backend.Stylo.Application.Interfaces;
using Stylo.Backend.Stylo.Domain.Entities;
using Stylo.Backend.Stylo.Infrastructure.Data;

namespace Stylo.Backend.Stylo.Infrastructure.Repositories
{
    public class WebsiteFeedbackRepository : IWebsiteFeedbackRepository
    {
        private readonly AppDbContext _context;

        public WebsiteFeedbackRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<WebsiteFeedback?> GetByIdAsync(int id)
        {
            return await _context.WebsiteFeedbacks.FindAsync(id);
        }

        public async Task<IEnumerable<WebsiteFeedback>> GetAllAsync()
        {
            return await _context.WebsiteFeedbacks
                .Include(f => f.User)
                .ToListAsync();
        }

        public async Task<IEnumerable<WebsiteFeedback>> GetFeaturedAsync()
        {
            return await _context.WebsiteFeedbacks
                .Include(f => f.User)
                .Where(f => f.IsFeatured)
                .ToListAsync();
        }

        public async Task AddAsync(WebsiteFeedback feedback)
        {
            await _context.WebsiteFeedbacks.AddAsync(feedback);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
