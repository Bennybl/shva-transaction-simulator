using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shva.TransactionSimulator.Api.Application.Interfaces;
using Shva.TransactionSimulator.Api.Contracts.Requests;
using Shva.TransactionSimulator.Api.Contracts.Responses;

namespace Shva.TransactionSimulator.Api.Controllers;

[ApiController]
[Route("api/transactions")]
[Authorize]
public sealed class TransactionsController(ITransactionApprovalService approvalService) : ControllerBase
{
    /// <summary>
    /// Simulates a transaction. A rejected transaction is still a successful simulation (200);
    /// 400 is returned only for invalid input.
    /// </summary>
    [HttpPost("simulate")]
    [ProducesResponseType<TransactionSimulationResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Simulate(
        [FromBody] SimulateTransactionRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await approvalService.SimulateAsync(request, GetUserId(), cancellationToken);
            return Ok(result);
        }
        catch (UnsupportedRegionException ex)
        {
            ModelState.AddModelError(nameof(request.RegionId), ex.Message);
            return ValidationProblem(ModelState);
        }
    }

    [HttpGet("approved")]
    [ProducesResponseType<IReadOnlyList<ApprovedTransactionResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetApproved(
        [FromQuery] int limit = 20,
        CancellationToken cancellationToken = default)
    {
        var transactions = await approvalService.GetApprovedAsync(limit, cancellationToken);
        return Ok(transactions);
    }

    private Guid? GetUserId() =>
        Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;
}
