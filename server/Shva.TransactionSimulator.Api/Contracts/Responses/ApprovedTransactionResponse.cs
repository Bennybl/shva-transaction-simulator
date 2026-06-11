namespace Shva.TransactionSimulator.Api.Contracts.Responses;

public sealed record ApprovedTransactionResponse(
    Guid Id,
    string RegionName,
    string TimeZoneId,
    DateOnly LocalDate,
    string LocalTime,          // "HH:mm"
    DateTime CreatedAtUtc);
