using Shva.TransactionSimulator.Api.Application.Interfaces;
using Shva.TransactionSimulator.Api.Domain.Models;

namespace Shva.TransactionSimulator.Api.Application.Services;

public sealed class RegionService : IRegionService
{
    // USA is deliberately split by time zone — a single "USA" has no unambiguous local time.
    private static readonly IReadOnlyList<RegionDefinition> Regions =
    [
        new() { Id = "IL",   DisplayName = "Israel",        TimeZoneId = "Asia/Jerusalem" },
        new() { Id = "FR",   DisplayName = "France",        TimeZoneId = "Europe/Paris" },
        new() { Id = "CY",   DisplayName = "Cyprus",        TimeZoneId = "Asia/Nicosia" },
        new() { Id = "IT",   DisplayName = "Italy",         TimeZoneId = "Europe/Rome" },
        new() { Id = "JP",   DisplayName = "Japan",         TimeZoneId = "Asia/Tokyo" },
        new() { Id = "US-E", DisplayName = "USA - Eastern", TimeZoneId = "America/New_York" },
        new() { Id = "US-P", DisplayName = "USA - Pacific", TimeZoneId = "America/Los_Angeles" }
    ];

    public IReadOnlyList<RegionDefinition> GetAll() => Regions;

    public RegionDefinition? GetById(string regionId) =>
        Regions.FirstOrDefault(r => string.Equals(r.Id, regionId, StringComparison.OrdinalIgnoreCase));
}
