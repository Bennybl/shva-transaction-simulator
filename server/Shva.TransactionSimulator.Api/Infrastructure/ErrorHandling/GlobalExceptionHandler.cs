using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Shva.TransactionSimulator.Api.Infrastructure.ErrorHandling;

/// <summary>
/// Single place that turns exceptions escaping the MVC pipeline into RFC 7807
/// ProblemDetails responses. Known cases (e.g. a unique-constraint violation from a
/// concurrent insert) map to a meaningful status; anything else becomes a logged 500.
/// Wired up via <c>AddExceptionHandler</c> + <c>UseExceptionHandler</c> in Program.cs.
/// </summary>
public sealed class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger,
    IHostEnvironment environment,
    IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    // SQL Server error numbers for unique-index / unique-constraint violations.
    private const int SqlDuplicateKeyInUniqueIndex = 2601;
    private const int SqlUniqueConstraintViolation = 2627;

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (status, title) = Map(exception);

        // Expected, mapped cases (4xx) are noise at Error level; only true 500s are errors.
        if (status >= StatusCodes.Status500InternalServerError)
        {
            logger.LogError(
                exception,
                "Unhandled exception while processing {Method} {Path}",
                httpContext.Request.Method,
                httpContext.Request.Path);
        }
        else
        {
            logger.LogWarning(
                "Request {Method} {Path} failed with {Status}: {Title}",
                httpContext.Request.Method,
                httpContext.Request.Path,
                status,
                title);
        }

        httpContext.Response.StatusCode = status;

        var problemDetails = new ProblemDetails
        {
            Status = status,
            Title = title,
            // Only expose exception detail outside production to aid debugging.
            Detail = environment.IsDevelopment() ? exception.ToString() : null
        };

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = problemDetails
        });
    }

    /// <summary>Maps an exception to the HTTP status and client-facing title to return.</summary>
    private static (int Status, string Title) Map(Exception exception) => exception switch
    {
        DbUpdateException dbEx when IsUniqueViolation(dbEx) =>
            (StatusCodes.Status409Conflict, $"A {ResourceName(dbEx)} with the same value already exists."),
        _ =>
            (StatusCodes.Status500InternalServerError, "An unexpected error occurred."),
    };

    private static bool IsUniqueViolation(DbUpdateException exception) =>
        exception.InnerException is SqlException
        {
            Number: SqlDuplicateKeyInUniqueIndex or SqlUniqueConstraintViolation
        };

    /// <summary>
    /// The entity that failed to save, taken from EF's change-tracker metadata
    /// (e.g. "User"). Falls back to "resource" when EF didn't attach the entries.
    /// </summary>
    private static string ResourceName(DbUpdateException exception) =>
        exception.Entries.FirstOrDefault()?.Metadata.ClrType.Name ?? "resource";
}
