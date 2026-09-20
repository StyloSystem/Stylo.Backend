using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Stylo.Backend.Stylo.Domain.Entities;

namespace Stylo.Backend.Stylo.Infrastructure.Data.Configurations
{
    public class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.HasKey(p => p.Id);

            builder.Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(p => p.Description)
                .HasMaxLength(1000);

            builder.Property(p => p.Price)
                .HasColumnType("decimal(18,2)");

            builder.Property(p => p.Gender)
                .HasConversion<string>()
                .HasMaxLength(20);

            builder.Property(p => p.IsDeleted).HasDefaultValue(false);

            builder.Property(p => p.ImagePublicId)
                .HasMaxLength(500);

            
            builder.HasQueryFilter(p => !p.IsDeleted);


            builder.HasIndex(p => p.CategoryId);
        }
    }
}
