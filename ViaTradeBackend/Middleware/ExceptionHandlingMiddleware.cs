using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using ViaTradeBackend.Models.Exceptions;

namespace ViaTradeBackend.Middleware
{
    public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        private readonly RequestDelegate _next = next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger = logger;

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleException(context, ex);
            }
        }

        private async Task HandleException(HttpContext context, Exception exception)
        {
            var problem = exception switch
            {
                UnauthorizedAccessException => CreateProblem(
                    StatusCodes.Status401Unauthorized,
                    "Unauthorized",
                    "https://httpstatuses.com/401  ",
                    "Invalid login or password"
                ),

                ForbiddenException => CreateProblem(
                    StatusCodes.Status403Forbidden,
                    "Forbidden",
                    "https://httpstatuses.com/403  ",
                    "Access denied to the requested resource"
                ),

                KeyNotFoundException => CreateProblem(
                    StatusCodes.Status404NotFound,
                    "Not Found",
                    "https://httpstatuses.com/404  ",
                    "The requested resource does not exist"
                ),

                ArgumentException => CreateProblem(
                    StatusCodes.Status400BadRequest,
                    "Bad Request",
                    "https://httpstatuses.com/400  ",
                    exception.Message
                ),

                InvalidOperationException => CreateProblem(
                    StatusCodes.Status409Conflict,
                    "Invalid/Conflict Operation",
                    "https://httpstatuses.com/409",
                    "Operation is not valid due to the current state of the object"
                ),

                // 408 Canceled or server timeout
                OperationCanceledException or TaskCanceledException => CreateProblem(
                    StatusCodes.Status408RequestTimeout,
                    "Request Cancelled",
                    "https://httpstatuses.com/408  ",
                    "Client closed the request or the server timeout has expired"
                ),

#if !DEBUG
                _ => HandleAll(),
#else
        _ => throw exception // In DEBUG mode, propagate exception for debugging
#endif
            };

            context.Response.StatusCode = problem.Status!.Value;
            context.Response.ContentType = "application/problem+json";

            _logger.LogInformation(
                "Exception handled: Status={Status}, Path={Path}, Type={ExceptionType}",
                problem.Status,
                context.Request.Path,
                exception.GetType().Name
            );

            await context.Response.WriteAsync(
                JsonSerializer.Serialize(problem)
            );
        }

        private static ProblemDetails HandleAll()
        {
            return CreateProblem(
                StatusCodes.Status500InternalServerError,
                "Internal Server Error",
                "https://httpstatuses.com/500",
                "Unexpected server error"
            );
        }

        private static ProblemDetails CreateProblem(
            int status,
            string title,
            string type,
            string detail
        )
        {
            return new ProblemDetails
            {
                Status = status,
                Title = title,
                Type = type,
                Detail = detail
            };
        }
    }
}