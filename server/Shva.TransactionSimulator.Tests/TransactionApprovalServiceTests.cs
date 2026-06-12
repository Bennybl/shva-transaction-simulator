using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Shva.TransactionSimulator.Api.Application.Interfaces;
using Shva.TransactionSimulator.Api.Application.Services;
using Shva.TransactionSimulator.Api.Contracts.Requests;
using Shva.TransactionSimulator.Api.Domain.Enums;
using Shva.TransactionSimulator.Api.Infrastructure.Persistence;

namespace Shva.TransactionSimulator.Tests;

public sealed class FakeClock : IClock
{
    public DateTimeOffset UtcNow { get; set; } = new(2026, 6, 11, 12, 0, 0, TimeSpan.Zero);
}

public sealed class TransactionApprovalServiceTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly AppDbContext _dbContext;
    private readonly FakeClock _clock = new();
    private readonly TransactionApprovalService _service;

    public TransactionApprovalServiceTests()
    {
        // SQLite in-memory: a relational provider, closer to MSSQL than EF InMemory.
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        _dbContext = new AppDbContext(options);
        _dbContext.Database.EnsureCreated();

        _service = new TransactionApprovalService(
            _dbContext,
            new RegionService(),
            _clock,
            NullLogger<TransactionApprovalService>.Instance);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        _connection.Dispose();
    }

    // --- Banking-hours boundary rule (pure decision logic) ---

    [Theory]
    [InlineData(7, 59, TransactionStatus.Rejected)]
    [InlineData(8, 0, TransactionStatus.Approved)]
    [InlineData(12, 0, TransactionStatus.Approved)]
    [InlineData(17, 59, TransactionStatus.Approved)]
    [InlineData(18, 0, TransactionStatus.Rejected)]
    [InlineData(18, 1, TransactionStatus.Rejected)]
    [InlineData(0, 0, TransactionStatus.Rejected)]
    [InlineData(23, 59, TransactionStatus.Rejected)]
    public void Decide_AppliesBankingHoursBoundaries(int hour, int minute, TransactionStatus expected)
    {
        var result = TransactionApprovalService.Decide(new TimeOnly(hour, minute));

        Assert.Equal(expected, result);
    }

    // --- Timezone conversion: the submitted instant is converted to the region's local time ---

    [Fact]
    public async Task Simulate_ConvertsInstantToRegionLocalTime_Japan()
    {
        // 01:00 UTC on a June day is 10:00 in Japan (UTC+9) -> within banking hours.
        var response = await _service.SimulateAsync(
            new SimulateTransactionRequest
            {
                RegionId = "JP",
                SubmittedAt = new DateTimeOffset(2026, 6, 11, 1, 0, 0, TimeSpan.Zero)
            },
            userId: null,
            CancellationToken.None);

        Assert.Equal(TransactionStatus.Approved, response.Status);
        Assert.Equal("10:00", response.LocalTime);
        Assert.Equal(new DateOnly(2026, 6, 11), response.LocalDate);
    }

    [Fact]
    public async Task Simulate_ConvertsInstantToRegionLocalTime_Us_DateRollsBack()
    {
        // 01:00 UTC on June 11 is 21:00 on June 10 in New York (UTC-4, DST) -> Rejected, previous local date.
        var response = await _service.SimulateAsync(
            new SimulateTransactionRequest
            {
                RegionId = "US",
                SubmittedAt = new DateTimeOffset(2026, 6, 11, 1, 0, 0, TimeSpan.Zero)
            },
            userId: null,
            CancellationToken.None);

        Assert.Equal(TransactionStatus.Rejected, response.Status);
        Assert.Equal("21:00", response.LocalTime);
        Assert.Equal(new DateOnly(2026, 6, 10), response.LocalDate);
    }

    [Fact]
    public async Task Simulate_Us_RejectedAtClosingBoundary()
    {
        // 22:00 UTC on June 11 is exactly 18:00 in New York (UTC-4, DST) -> 18:00 is exclusive -> Rejected.
        var response = await _service.SimulateAsync(
            new SimulateTransactionRequest
            {
                RegionId = "US",
                SubmittedAt = new DateTimeOffset(2026, 6, 11, 22, 0, 0, TimeSpan.Zero)
            },
            userId: null,
            CancellationToken.None);

        Assert.Equal(TransactionStatus.Rejected, response.Status);
        Assert.Equal("18:00", response.LocalTime);
        Assert.Equal(new DateOnly(2026, 6, 11), response.LocalDate);
    }

    [Fact]
    public async Task Simulate_RespectsSubmittedOffset()
    {
        // 14:24 at +03:00 (Israel summer time) is 11:24 UTC -> 13:24 in France (UTC+2, DST) -> Approved.
        var response = await _service.SimulateAsync(
            new SimulateTransactionRequest
            {
                RegionId = "FR",
                SubmittedAt = new DateTimeOffset(2026, 6, 11, 14, 24, 0, TimeSpan.FromHours(3))
            },
            userId: null,
            CancellationToken.None);

        Assert.Equal(TransactionStatus.Approved, response.Status);
        Assert.Equal("13:24", response.LocalTime);
    }

    [Fact]
    public async Task Simulate_UnknownRegion_Throws()
    {
        await Assert.ThrowsAsync<UnsupportedRegionException>(() =>
            _service.SimulateAsync(
                new SimulateTransactionRequest
                {
                    RegionId = "XX",
                    SubmittedAt = new DateTimeOffset(2026, 6, 11, 12, 0, 0, TimeSpan.Zero)
                },
                userId: null,
                CancellationToken.None));
    }

    // --- Persistence and retrieval ---

    [Fact]
    public async Task Simulate_PersistsBothApprovedAndRejected()
    {
        await SimulateAtUtcAsync("IL", 10, 0);  // 13:00 Israel -> Approved
        await SimulateAtUtcAsync("IL", 22, 0);  // 01:00 Israel -> Rejected

        var all = await _dbContext.Transactions.AsNoTracking().ToListAsync();

        Assert.Equal(2, all.Count);
        Assert.Contains(all, t => t.Status == TransactionStatus.Approved);
        Assert.Contains(all, t => t.Status == TransactionStatus.Rejected);
    }

    [Fact]
    public async Task GetApproved_ReturnsOnlyApproved_NewestFirst()
    {
        _clock.UtcNow = new DateTimeOffset(2026, 6, 11, 12, 0, 0, TimeSpan.Zero);
        var first = await SimulateAtUtcAsync("IL", 10, 0);   // Approved

        _clock.UtcNow = _clock.UtcNow.AddMinutes(1);
        await SimulateAtUtcAsync("IL", 22, 0);               // Rejected

        _clock.UtcNow = _clock.UtcNow.AddMinutes(1);
        var second = await SimulateAtUtcAsync("FR", 10, 0);  // Approved

        var approved = await _service.GetApprovedAsync(20, CancellationToken.None);

        Assert.Equal(2, approved.Count);
        Assert.Equal(second.Id, approved[0].Id); // newest first
        Assert.Equal(first.Id, approved[1].Id);
        Assert.DoesNotContain(approved, t => t.LocalTime == "01:00");
    }

    [Fact]
    public async Task GetApproved_RespectsLimit()
    {
        for (var i = 0; i < 5; i++)
        {
            _clock.UtcNow = _clock.UtcNow.AddMinutes(1);
            await SimulateAtUtcAsync("IL", 10, i); // all approved
        }

        var approved = await _service.GetApprovedAsync(3, CancellationToken.None);

        Assert.Equal(3, approved.Count);
    }

    private async Task<Shva.TransactionSimulator.Api.Contracts.Responses.TransactionSimulationResponse>
        SimulateAtUtcAsync(string regionId, int utcHour, int utcMinute) =>
        await _service.SimulateAsync(
            new SimulateTransactionRequest
            {
                RegionId = regionId,
                SubmittedAt = new DateTimeOffset(2026, 6, 11, utcHour, utcMinute, 0, TimeSpan.Zero)
            },
            userId: null,
            CancellationToken.None);
}
