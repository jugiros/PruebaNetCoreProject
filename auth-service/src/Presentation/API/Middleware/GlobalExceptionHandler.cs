using AuthService.Domain.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace AuthService.API.Middleware;

public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        ArgumentNullException.ThrowIfNull(logger);
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        var (status, title, detail) = exception switch
        {
            SecurityTokenExpiredException => (StatusCodes.Status401Unauthorized,  "Token expirado.",         "El token JWT ha expirado."),
            SecurityTokenException        => (StatusCodes.Status401Unauthorized,  "Token inválido.",         "El token JWT no es válido."),
            UnauthorizedAccessException   => (StatusCodes.Status403Forbidden,     "Acceso denegado.",        "No tiene permisos para esta operación."),
            DomainException ex            => (StatusCodes.Status422UnprocessableEntity, "Regla de negocio.", ex.Message),
            ValidationException ex        => (StatusCodes.Status400BadRequest,    "Errores de validación.", string.Join("; ", ex.Errors.Select(e => e.ErrorMessage))),
            _                             => (StatusCodes.Status500InternalServerError, "Error interno.",    "Error interno del servidor.")
        };

        if (status == StatusCodes.Status500InternalServerError)
            _logger.LogError(exception, "Error no controlado");

        httpContext.Response.StatusCode = status;
        await httpContext.Response.WriteAsJsonAsync(
            new ProblemDetails
            {
                Status = status,
                Title  = title,
                Detail = detail
            }, cancellationToken).ConfigureAwait(false);

        return true;
    }
}
