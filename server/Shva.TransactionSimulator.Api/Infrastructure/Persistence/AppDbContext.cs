using Microsoft.EntityFrameworkCore;
using Shva.TransactionSimulator.Api.Domain.Entities;
using Shva.TransactionSimulator.Api.Infrastructure.Persistence.Converters;

namespace Shva.TransactionSimulator.Api.Infrastructure.Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<User> Users => Set<User>();

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        // Every DateTime column in this model is a UTC instant. The store doesn't persist
        // the Kind, so mark it as UTC on read — otherwise it serializes to JSON without a
        // 'Z' and clients misread it as local time.
        configurationBuilder.Properties<DateTime>().HaveConversion<UtcDateTimeConverter>();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
