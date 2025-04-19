using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Domain.Models;

namespace Infrastructure.Configurations
{
    public class SummaryConfiguration : IEntityTypeConfiguration<Summary>
    {
        public void Configure(EntityTypeBuilder<Summary> builder)
        {
            builder.HasIndex(s => s.CreatedAt);

            builder.HasOne(s => s.Category)
                .WithMany(c => c.Summaries)
                .HasForeignKey(s => s.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Property(s => s.ChatName)
                .IsRequired()
                .HasMaxLength(200);
      
        }
    }
}
