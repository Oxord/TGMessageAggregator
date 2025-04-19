using MessageAggregator.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace MessageAggregator.Infrastructure;

public class AppDbContext : DbContext
{
    public DbSet<Summary> Summaries { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {}
        // Removed DbSet<Category>

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}