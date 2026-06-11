using Microsoft.EntityFrameworkCore;
using Shva.TransactionSimulator.Api.Application.Interfaces;
using Shva.TransactionSimulator.Api.Contracts.Requests;
using Shva.TransactionSimulator.Api.Contracts.Responses;
using Shva.TransactionSimulator.Api.Domain.Entities;
using Shva.TransactionSimulator.Api.Domain.Enums;
using Shva.TransactionSimulator.Api.Infrastructure.Persistence;
using TimeZoneConverter;

namespace Shva.TransactionSimulator.Api.Application.Services;

public sealed class TransactionApprovalService(
    AppDbContext dbContext,
    IRegionService regionService,
    IClock clock,
    ILogger<TransactionApprovalService> logger) : ITransactionApprovalService
{
    // Standard banking hours: start inclusive, end exclusive.
    public static readonly TimeOnly BankingStart = new(8, 0);
    public static readonly TimeOnly BankingEnd = new(18, 0);

    private const int DefaultLimit = 20;
    private const int MaxLimit = 100;

    public async Task<TransactionSimulationResponse> SimulateAsync(
        SimulateTransactionRequest request,
        Guid? userId,
        CancellationToken cancellationToken)
    {
        var region = regionService.GetById(request.RegionId)
            ?? throw new UnsupportedRegionException(request.RegionId);

        var submittedAtUtc = request.SubmittedAt!.Value.UtcDateTime;

        // Determine what the local time was in the selected region at that exact moment.
        // TimeZoneConverter resolves IANA ids on both Windows and Linux.
        var timeZone = TZConvert.GetTimeZoneInfo(region.TimeZoneId);
        var localDateTime = TimeZoneInfo.ConvertTimeFromUtc(submittedAtUtc, timeZone);

        var localDate = DateOnly.FromDateTime(localDateTime);
        var localTime = TimeOnly.FromDateTime(localDateTime);

        var status = Decide(localTime);
        var reason = status == TransactionStatus.Approved
            ? $"Approved: Local time {localTime:HH\\:mm} in {region.DisplayName} is within banking hours {BankingStart:HH\\:mm}-{BankingEnd:HH\\:mm}."
            : $"Rejected: Local time {localTime:HH\\:mm} in {region.DisplayName} is outside banking hours {BankingStart:HH\\:mm}-{BankingEnd:HH\\:mm}.";

        var transaction = new Transaction
        {
            Id = Guid.NewGuid(),
            RegionId = region.Id,
            RegionName = region.DisplayName,
            TimeZoneId = region.TimeZoneId,
            SubmittedAtUtc = submittedAtUtc,
            LocalDate = localDate,
            LocalTime = localTime,
            Status = status,
            DecisionReason = reason,
            CreatedAtUtc = clock.UtcNow.UtcDateTime,
            UserId = userId
        };

        dbContext.Transactions.Add(transaction);
        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Transaction simulated: Region={RegionId} LocalDate={LocalDate} LocalTime={LocalTime} Status={Status}",
            region.Id, localDate, localTime, status);

        return ToSimulationResponse(transaction);
    }

    /// <summary>Pure decision rule: approved iff local time is within [BankingStart, BankingEnd).</summary>
    public static TransactionStatus Decide(TimeOnly localTime) =>
        localTime >= BankingStart && localTime < BankingEnd
            ? TransactionStatus.Approved
            : TransactionStatus.Rejected;

    public async Task<IReadOnlyList<ApprovedTransactionResponse>> GetApprovedAsync(
        int limit,
        CancellationToken cancellationToken)
    {
        var effectiveLimit = limit <= 0 ? DefaultLimit : Math.Min(limit, MaxLimit);

        var transactions = await dbContext.Transactions
            .AsNoTracking()
            .Where(t => t.Status == TransactionStatus.Approved)
            .OrderByDescending(t => t.CreatedAtUtc)
            .Take(effectiveLimit)
            .ToListAsync(cancellationToken);

        // Time formatting is done in memory — not translatable to SQL.
        return transactions
            .Select(t => new ApprovedTransactionResponse(
                t.Id,
                t.RegionName,
                t.TimeZoneId,
                t.LocalDate,
                t.LocalTime.ToString("HH:mm"),
                t.CreatedAtUtc))
            .ToList();
    }

    private static TransactionSimulationResponse ToSimulationResponse(Transaction t) => new(
        t.Id,
        t.RegionId,
        t.RegionName,
        t.TimeZoneId,
        t.LocalDate,
        t.LocalTime.ToString("HH:mm"),
        t.Status,
        t.DecisionReason,
        t.SubmittedAtUtc,
        t.CreatedAtUtc);
}
