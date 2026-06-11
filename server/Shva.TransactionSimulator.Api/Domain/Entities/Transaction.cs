using Shva.TransactionSimulator.Api.Domain.Enums;

namespace Shva.TransactionSimulator.Api.Domain.Entities;

public sealed class Transaction
{
    public Guid Id { get; set; }

    public string RegionId { get; set; } = null!;
    public string RegionName { get; set; } = null!;
    public string TimeZoneId { get; set; } = null!;

    /// <summary>The submitted instant, normalized to UTC.
    /// Stored as DateTime (UTC) — sortable on every relational provider.</summary>
    public DateTime SubmittedAtUtc { get; set; }

    /// <summary>Region-local date/time computed by the backend at the submitted instant.</summary>
    public DateOnly LocalDate { get; set; }
    public TimeOnly LocalTime { get; set; }

    public TransactionStatus Status { get; set; }
    public string DecisionReason { get; set; } = null!;

    public DateTime CreatedAtUtc { get; set; }

    public Guid? UserId { get; set; }
}
