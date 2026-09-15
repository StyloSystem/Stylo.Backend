using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Stylo.Backend.Stylo.Domain.Entities;

namespace Stylo.Backend.Stylo.Infrastructure.Data.Configurations
{
    public class WebsiteFeedbackConfiguration : IEntityTypeConfiguration<WebsiteFeedback>
    {
        public void Configure(EntityTypeBuilder<WebsiteFeedback> builder)
        {
            builder.Property(wf => wf.Message)
                   .IsRequired()
                   .HasMaxLength(1000);
        }
    }
}
