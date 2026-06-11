using System.ComponentModel.DataAnnotations;

namespace Shva.TransactionSimulator.Api.Contracts.Requests;

public sealed class SimulateTransactionRequest
{
    [Required]
    public string RegionId { get; init; } = null!;

    /// <summary>
    /// The instant the transaction is simulated at, ISO-8601 with offset
    /// (e.g. "2026-06-11T14:24:00+03:00"). The backend converts this instant
    /// to the selected region's local time to apply the banking-hours rule.
    /// </summary>
    [Required]
    public DateTimeOffset? SubmittedAt { get; init; }
}
