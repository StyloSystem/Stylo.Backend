using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Stylo.Backend.Stylo.Domain.Entities;

namespace Stylo.Backend.Stylo.Infrastructure.Data.Configurations
{
    public class ProductSizeConfiguration : IEntityTypeConfiguration<ProductSize>
    {
        public void Configure(EntityTypeBuilder<ProductSize> builder)
        {
            builder.HasKey(ps => ps.Id);

            builder.Property(ps => ps.Size)
                .IsRequired()
                .HasConversion<string>();

            builder.Property(ps => ps.RowVersion)
                .IsRowVersion();

            builder.Ignore(ps => ps.IsAvailable);

            builder.HasOne(ps => ps.Product)
                .WithMany(p => p.ProductSizes)
                .HasForeignKey(ps => ps.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(ps => ps.ProductId);
        }
    }
}
