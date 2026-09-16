using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Stylo.Backend.Stylo.Domain.Entities;

namespace Stylo.Backend.Stylo.Infrastructure.Data.Configurations
{
    public class OrderConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.HasKey(o => o.Id);

            builder.Property(o => o.RecipientName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(o => o.ContactPhone)
                .IsRequired()
                .HasMaxLength(30);

            builder.Property(o => o.ShippingAddress)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(o => o.PaymentMethod)
                .HasConversion<string>()
                .HasMaxLength(50);

            builder.Property(o => o.Status)
                .HasConversion<string>()
                .HasMaxLength(50);

            builder.Property(o => o.TotalPrice)
                .HasColumnType("decimal(18,2)");

            builder.HasOne(o => o.User)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(o => o.UserId);
        }
    }
}
