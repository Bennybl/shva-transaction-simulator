using Shva.TransactionSimulator.Api.Domain.Models;

namespace Shva.TransactionSimulator.Api.Application.Interfaces;

public interface IRegionService
{
    IReadOnlyList<RegionDefinition> GetAll();
    RegionDefinition? GetById(string regionId);
}
