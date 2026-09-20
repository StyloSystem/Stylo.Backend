using Stylo.Backend.Stylo.Domain.Entities;

namespace Stylo.Backend.Stylo.Application.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByIdAsync(int id);
        Task<bool> EmailExistsAsync(string email);
        Task<bool> CreateUserAsync(User user, string password);
        Task<bool> CreateUserWithHashedPasswordAsync(User user);
        Task<bool> UpdateUserAsync(User user);
        Task<bool> CheckPasswordAsync(User user, string password);
        Task<bool> UpdatePasswordAsync(User user, string newPassword);
    }
}
