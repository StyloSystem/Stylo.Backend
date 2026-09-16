using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Stylo.Backend.Stylo.Domain.Entities;

namespace Stylo.Backend.Stylo.Infrastructure.Data.Configurations
{
    public class FavouriteConfiguration : IEntityTypeConfiguration<Favourite>
    {
        public void Configure(EntityTypeBuilder<Favourite> builder)
        {
            builder.HasIndex(f => new { f.UserId, f.ProductId }).IsUnique();

            builder.HasOne(f => f.User)
                   .WithMany(u => u.Favourites)
                   .HasForeignKey(f => f.UserId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(f => f.Product)
                   .WithMany(p => p.Favourites)
                   .HasForeignKey(f => f.ProductId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
