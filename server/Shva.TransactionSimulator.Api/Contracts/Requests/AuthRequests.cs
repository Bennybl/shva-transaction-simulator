using System.ComponentModel.DataAnnotations;

namespace Shva.TransactionSimulator.Api.Contracts.Requests;

public sealed class SignupRequest
{
    [Required, MinLength(3), MaxLength(50)]
    public string Username { get; init; } = null!;

    [Required, MinLength(6), MaxLength(100)]
    public string Password { get; init; } = null!;
}

public sealed class LoginRequest
{
    [Required]
    public string Username { get; init; } = null!;

    [Required]
    public string Password { get; init; } = null!;
}
