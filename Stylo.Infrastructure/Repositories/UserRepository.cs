using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Stylo.Backend.Stylo.Application.Exceptions;
using Stylo.Backend.Stylo.Application.Interfaces;
using Stylo.Backend.Stylo.Domain.Entities;
using Stylo.Backend.Stylo.Infrastructure.Data;

namespace Stylo.Backend.Stylo.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly UserManager<User> _userManager;
        private readonly AppDbContext _context;
        private readonly IPasswordHasher<User> _passwordHasher;

        public UserRepository(UserManager<User> userManager, AppDbContext context)
        {
            _userManager = userManager;
            _context = context;
            _passwordHasher = new PasswordHasher<User>();
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return null;
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email || u.NormalizedEmail == email);
            if (user != null && string.Equals(user.Email, email, StringComparison.Ordinal))
            {
                return user;
            }
            return null;
        }

        public async Task<User?> GetByIdAsync(int id)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email || u.NormalizedEmail == email);
            return user != null && string.Equals(user.Email, email, StringComparison.Ordinal);
        }

        public async Task<bool> CreateUserAsync(User user, string password)
        {
            user.UserName = user.Email;
            if (user.Email != null)
            {
                user.NormalizedEmail = user.Email;
                user.NormalizedUserName = user.Email;
            }
            user.SecurityStamp = Guid.NewGuid().ToString();
            user.ConcurrencyStamp = Guid.NewGuid().ToString();
            user.PasswordHash = _passwordHasher.HashPassword(user, password);

            var result = await _userManager.CreateAsync(user);
            if (!result.Succeeded)
            {
                var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                throw new BadRequestException($"User creation failed: {errors}");
            }

            return true;
        }

        public async Task<bool> CreateUserWithHashedPasswordAsync(User user)
        {
            user.UserName = user.Email;
            if (user.Email != null)
            {
                user.NormalizedEmail = user.Email.ToUpperInvariant();
                user.NormalizedUserName = user.Email.ToUpperInvariant();
            }
            user.SecurityStamp = Guid.NewGuid().ToString();
            user.ConcurrencyStamp = Guid.NewGuid().ToString();

            var result = await _userManager.CreateAsync(user);
            if (!result.Succeeded)
            {
                var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                throw new BadRequestException($"User creation failed: {errors}");
            }

            return true;
        }

        public async Task<bool> UpdateUserAsync(User user)
        {
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                throw new BadRequestException($"User update failed: {errors}");
            }

            return true;
        }

        public async Task<bool> CheckPasswordAsync(User user, string password)
        {
            if (string.IsNullOrEmpty(user.PasswordHash)) return false;
            var verificationResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
            return verificationResult == PasswordVerificationResult.Success || verificationResult == PasswordVerificationResult.SuccessRehashNeeded;
        }

        public async Task<bool> UpdatePasswordAsync(User user, string newPassword)
        {
            user.PasswordHash = _passwordHasher.HashPassword(user, newPassword);
            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded;
        }
    }
}
