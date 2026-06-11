using Shva.TransactionSimulator.Api.Domain.Enums;

namespace Shva.TransactionSimulator.Api.Contracts.Responses;

public sealed record TransactionSimulationResponse(
    Guid Id,
    string RegionId,
    string RegionName,
    string TimeZoneId,
    DateOnly LocalDate,
    string LocalTime,          // "HH:mm"
    TransactionStatus Status,  // serialized as string
    string DecisionReason,
    DateTime SubmittedAtUtc,
    DateTime CreatedAtUtc);
