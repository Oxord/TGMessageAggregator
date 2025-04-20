using MessageAggregator.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MessageAggregator.Infrastructure.Configurations;

public class SummaryConfiguration : IEntityTypeConfiguration<Summary>
{
    public void Configure(EntityTypeBuilder<Summary> builder)
    {
        builder.HasIndex(s => s.CreatedAt);

            // Removed Category relationship configuration

            builder.Property(s => s.CategoryName) // Added configuration for CategoryName
                .IsRequired()
                 .HasMaxLength(100); // Assuming a max length for the category name

            builder.Property(s => s.ChatName)
                // .IsRequired() // Removed to make ChatName nullable
                .HasMaxLength(200);

    }
}