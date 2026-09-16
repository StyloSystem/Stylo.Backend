using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Stylo.Backend.Stylo.Domain.Entities;

namespace Stylo.Backend.Stylo.Infrastructure.Data.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.Property(u => u.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(u => u.Role)
                .IsRequired()
                .HasMaxLength(50);

            var adminUser = new User
            {
                Id = 1,
                UserName = "admin@stylo.com",
                NormalizedUserName = "ADMIN@STYLO.COM",
                Email = "admin@stylo.com",
                NormalizedEmail = "ADMIN@STYLO.COM",
                EmailConfirmed = true,
                Name = "System Admin",
                Role = "Admin",
                PasswordHash = "AQAAAAIAAYagAAAAEBwSpRa+1gB8XDu/uTYun+CuLqAhgM6Hez9NJBbXaYtQ1bahvj7lySAEPfos7Ms2Qg==",
                SecurityStamp = "00000000-0000-0000-0000-000000000001",
                ConcurrencyStamp = "00000000-0000-0000-0000-000000000001"
            };

            builder.HasData(adminUser);
        }
    }
}
