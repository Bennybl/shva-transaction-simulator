using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shva.TransactionSimulator.Api.Domain.Entities;

namespace Shva.TransactionSimulator.Api.Infrastructure.Persistence.EntityConfigurations;

public sealed class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.ToTable("Transactions");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.RegionId).IsRequired().HasMaxLength(50);
        builder.Property(t => t.RegionName).IsRequired().HasMaxLength(100);
        builder.Property(t => t.TimeZoneId).IsRequired().HasMaxLength(100);

        // Stored as string for easy inspection during review.
        builder.Property(t => t.Status)
            .IsRequired()
            .HasMaxLength(20)
            .HasConversion<string>();

        builder.Property(t => t.DecisionReason).IsRequired().HasMaxLength(250);

        // The approved-transactions endpoint filters by status and sorts by creation time.
        builder.HasIndex(t => new { t.Status, t.CreatedAtUtc })
            .HasDatabaseName("IX_Transactions_Status_CreatedAtUtc");
    }
}
