using System.Text.Json;
using ExceptionTools;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace GlobalExceptionHandler;

internal sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler {
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken) {
        ProblemDetails problemDetails;
        if (exception is BaseApplicationException baseApplicationException) {
            logger.LogError(baseApplicationException, "Application exception. Type: {type} Code: {code} Message: {message}",
                baseApplicationException.Type, baseApplicationException.Code, baseApplicationException.Message);
            
            problemDetails = Problem(baseApplicationException);
            if (baseApplicationException.Type == ErrorType.Validation) {
                ValidationException validationException = (ValidationException)exception;
                problemDetails.Extensions["errors"] = validationException.Errors;
            } else {
                problemDetails.Extensions["details"] = baseApplicationException.Details;
            }
        } else {
            logger.LogError(exception, "Unhandled exception has been occured: {Message}", exception.Message);
            problemDetails = InternalServerProblem();
        }

        problemDetails.Instance = (string)httpContext.Request.Path;
        problemDetails.Extensions["traceId"] = httpContext.TraceIdentifier;
        httpContext.Response.StatusCode = problemDetails.Status!.Value;
        
        await httpContext.Response.WriteAsync(JsonSerializer.Serialize(problemDetails), cancellationToken);
        
        return true;
    }

    private static ProblemDetails Problem(BaseApplicationException exception) =>
        exception.Type switch {
            ErrorType.Conflict => new ProblemDetails {
                Title = exception.Code,
                Detail = exception.Message,
                Status = 409,
                Type = "https://datatracker.ietf.org/doc/html/rfc9110#section-15.5.10"
            },
            ErrorType.NotFound => new ProblemDetails {
                Title = exception.Code,
                Detail = exception.Message,
                Status = 404,
                Type = "https://datatracker.ietf.org/doc/html/rfc9110#section-15.5.5"
            },
            ErrorType.Unauthorized => new ProblemDetails {
                Title = exception.Code,
                Detail = exception.Message,
                Status = 401,
                Type = "https://datatracker.ietf.org/doc/html/rfc9110#section-15.5.2"
            },
            ErrorType.Validation => new ProblemDetails {
                Title = "One or more validation errors",
                Detail = null,
                Status = 400,
                Type = "https://datatracker.ietf.org/doc/html/rfc9110#section-15.5.1"
            },
            _ => InternalServerProblem()
        };

    private static ProblemDetails InternalServerProblem() =>
        new ProblemDetails {
            Title = "Internal server error",
            Status = 500
        };
}