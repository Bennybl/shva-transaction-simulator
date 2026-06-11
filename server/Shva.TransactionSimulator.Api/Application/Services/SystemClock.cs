using Shva.TransactionSimulator.Api.Application.Interfaces;

namespace Shva.TransactionSimulator.Api.Application.Services;

public sealed class SystemClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
