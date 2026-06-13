using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Shva.TransactionSimulator.Api.Infrastructure.Persistence.Converters;

/// <summary>
/// Forces DateTime values to be read back with <see cref="DateTimeKind.Utc"/>.
/// SQL Server's <c>datetime2</c> (and SQLite's TEXT) don't persist the Kind, so EF
/// materializes them as <see cref="DateTimeKind.Unspecified"/> — which then serializes
/// to JSON without a trailing 'Z', making UTC instants look like local time to clients.
/// Every DateTime column in this model is a UTC instant, so this is applied globally
/// via <c>ConfigureConventions</c> in <see cref="AppDbContext"/>.
/// </summary>
public sealed class UtcDateTimeConverter : ValueConverter<DateTime, DateTime>
{
    public UtcDateTimeConverter()
        : base(
            write => write,
            read => DateTime.SpecifyKind(read, DateTimeKind.Utc))
    {
    }
}
