using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Stylo.Backend.Stylo.Domain.Entities;

namespace Stylo.Backend.Stylo.Infrastructure.Data.Configurations
{
    public class ProductFeedbackConfiguration : IEntityTypeConfiguration<ProductFeedback>
    {
        public void Configure(EntityTypeBuilder<ProductFeedback> builder)
        {
            
            builder.HasIndex(pf => new { pf.UserId, pf.ProductId }).IsUnique();

            builder.Property(pf => pf.Message)
                   .HasMaxLength(1000);
        }
    }
}
