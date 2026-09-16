using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Stylo.Backend.Stylo.Domain.Entities;

namespace Stylo.Backend.Stylo.Infrastructure.Data.Configurations
{
    public class ProductFeedbackConfiguration : IEntityTypeConfiguration<ProductFeedback>
    {
        public void Configure(EntityTypeBuilder<ProductFeedback> builder)
        {
            builder.HasKey(pf => pf.Id);

            builder.Property(pf => pf.Message)
                .IsRequired()
                .HasMaxLength(2000);

            builder.HasOne(pf => pf.User)
                .WithMany(u => u.ProductFeedbacks)
                .HasForeignKey(pf => pf.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(pf => pf.Product)
                .WithMany(p => p.ProductFeedbacks)
                .HasForeignKey(pf => pf.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(pf => pf.Order)
                .WithMany(o => o.ProductFeedbacks)
                .HasForeignKey(pf => pf.OrderId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(pf => pf.UserId);
            builder.HasIndex(pf => pf.ProductId);
            builder.HasIndex(pf => pf.OrderId);

            builder.HasIndex(pf => new { pf.UserId, pf.ProductId })
                .IsUnique();
        }
    }
}
