namespace Shva.TransactionSimulator.Api.Domain.Models;

public sealed class RegionDefinition
{
    public string Id { get; init; } = null!;
    public string DisplayName { get; init; } = null!;

    /// <summary>IANA time zone identifier (e.g. "Asia/Jerusalem").</summary>
    public string TimeZoneId { get; init; } = null!;
}
