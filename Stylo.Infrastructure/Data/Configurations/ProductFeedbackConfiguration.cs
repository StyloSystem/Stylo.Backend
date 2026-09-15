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
                   .IsRequired()
                   .HasMaxLength(1000);

            builder.HasOne(pf => pf.User)
                   .WithMany(u => u.ProductFeedbacks)
                   .HasForeignKey(pf => pf.UserId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
