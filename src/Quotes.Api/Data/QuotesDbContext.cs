using Microsoft.EntityFrameworkCore;
using Quotes.Api.Models;

namespace Quotes.Api.Data;

public class QuotesDbContext : DbContext
{
    public QuotesDbContext(DbContextOptions<QuotesDbContext> options) : base(options)
    {

    }

    public DbSet<Quote> Quotes => Set<Quote>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Quote>(e =>
                {
                    e.HasKey(q => q.Id);
                    e.Property(q => q.Text).IsRequired().HasMaxLength(500);
                    e.Property(q => q.Author).IsRequired().HasMaxLength(120);
                    e.Property(q => q.CreatedAt).IsRequired();
                    e.HasIndex(q => q.Author);
                });
    }
}
