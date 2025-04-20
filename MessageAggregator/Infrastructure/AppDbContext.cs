using MessageAggregator.Domain.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore; // Added Identity using
using Microsoft.EntityFrameworkCore;

namespace MessageAggregator.Infrastructure;

// Changed base class to IdentityDbContext<User>
public class AppDbContext : IdentityDbContext<User>
{
    public DbSet<Summary> Summaries { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {}
        // Removed DbSet<Category>

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder); // Added call to base method for Identity configuration
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
