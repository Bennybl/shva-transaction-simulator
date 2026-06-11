using Microsoft.AspNetCore.Mvc;
using Shva.TransactionSimulator.Api.Application.Interfaces;
using Shva.TransactionSimulator.Api.Contracts.Responses;

namespace Shva.TransactionSimulator.Api.Controllers;

[ApiController]
[Route("api/regions")]
public sealed class RegionsController(IRegionService regionService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<RegionResponse>>(StatusCodes.Status200OK)]
    public IActionResult GetAll()
    {
        var regions = regionService.GetAll()
            .Select(r => new RegionResponse(r.Id, r.DisplayName, r.TimeZoneId))
            .ToList();

        return Ok(regions);
    }
}
