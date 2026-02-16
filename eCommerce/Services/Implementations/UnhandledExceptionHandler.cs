using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Services.Implementations
{
    /// <summary>
    /// Catch-all handler for non-domain exceptions. Logs the full exception
    /// and returns a generic 500 ProblemDetails — no stack traces leak to the client.
    /// Registered after DomainExceptionHandler so domain exceptions are handled first.
    /// </summary>
    public sealed class UnhandledExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<UnhandledExceptionHandler> logger;

        public UnhandledExceptionHandler(ILogger<UnhandledExceptionHandler> logger)
        {
            this.logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            logger.LogError(exception,
                "Unhandled exception on {Method} {Path}",
                httpContext.Request.Method,
                httpContext.Request.Path);

            httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

            await httpContext.Response.WriteAsJsonAsync(new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Internal Server Error",
                Detail = "An unexpected error occurred. Please try again later."
            }, cancellationToken);

            return true;
        }
    }
}
