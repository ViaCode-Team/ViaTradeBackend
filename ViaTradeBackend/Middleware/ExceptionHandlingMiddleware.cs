using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

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

        private static async Task HandleException(HttpContext context, Exception exception)
        {
            var problem = exception switch
            {
                UnauthorizedAccessException => CreateProblem(
                    StatusCodes.Status401Unauthorized,
                    "Unauthorized",
                    "https://httpstatuses.com/401",
                    "Invalid login or password"
                ),
                ArgumentException => CreateProblem(
                    StatusCodes.Status400BadRequest,
                    "Bad Request",
                    "https://httpstatuses.com/400",
                    exception.Message
                ),
#if !DEBUG
                _ => HandleAll(),
#else
                _ => throw exception // В DEBUG пробрасываем исключение для отладки
#endif
            };

            context.Response.StatusCode = problem.Status!.Value;
            context.Response.ContentType = "application/problem+json";

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