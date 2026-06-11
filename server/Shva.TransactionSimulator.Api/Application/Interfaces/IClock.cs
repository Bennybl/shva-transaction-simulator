namespace Shva.TransactionSimulator.Api.Application.Interfaces;

public interface IClock
{
    DateTimeOffset UtcNow { get; }
}
