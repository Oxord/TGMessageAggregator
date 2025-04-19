using MessageAggregator.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MessageAggregator.Infrastructure.Configurations;

public class SummaryConfiguration : IEntityTypeConfiguration<Summary>
{
    public void Configure(EntityTypeBuilder<Summary> builder)
    {
        builder.HasIndex(s => s.CreatedAt);

        builder.Property(s => s.ChatName)
            .IsRequired()
            .HasMaxLength(200);
      
    }
}