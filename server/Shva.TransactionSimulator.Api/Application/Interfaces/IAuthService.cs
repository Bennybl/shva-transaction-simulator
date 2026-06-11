using Shva.TransactionSimulator.Api.Contracts.Requests;
using Shva.TransactionSimulator.Api.Contracts.Responses;

namespace Shva.TransactionSimulator.Api.Application.Interfaces;

public interface IAuthService
{
    /// <returns>Auth response, or null if the username is already taken.</returns>
    Task<AuthResponse?> SignupAsync(SignupRequest request, CancellationToken cancellationToken);

    /// <returns>Auth response, or null if the credentials are invalid.</returns>
    Task<AuthResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken);
}
