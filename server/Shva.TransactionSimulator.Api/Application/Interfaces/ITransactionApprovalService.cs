using Shva.TransactionSimulator.Api.Contracts.Requests;
using Shva.TransactionSimulator.Api.Contracts.Responses;

namespace Shva.TransactionSimulator.Api.Application.Interfaces;

public interface ITransactionApprovalService
{
    /// <summary>
    /// Simulates a transaction: converts the submitted instant to the region's local time,
    /// applies the banking-hours rule, persists the result (approved or rejected) and returns it.
    /// </summary>
    /// <exception cref="UnsupportedRegionException">The region id is not supported.</exception>
    Task<TransactionSimulationResponse> SimulateAsync(
        SimulateTransactionRequest request,
        Guid? userId,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<ApprovedTransactionResponse>> GetApprovedAsync(
        int limit,
        CancellationToken cancellationToken);
}

public sealed class UnsupportedRegionException(string regionId)
    : Exception($"Unsupported regionId '{regionId}'.");
