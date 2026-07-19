using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace ViaTradeBackend.Middleware;

public class ProblemDetailsStatusCodeMiddleware(RequestDelegate next)
{
	public async Task Invoke(HttpContext context)
	{
		await next(context);

		// Intercept 401/403 status codes and wrap response body
		if (context.Response.StatusCode is StatusCodes.Status401Unauthorized or StatusCodes.Status403Forbidden)
		{
			// Skip if response already started (headers sent)
			if (context.Response.HasStarted)
				return;

			try
			{
				context.Response.ContentType = "application/problem+json";

				var problem =
					context.Response.StatusCode == StatusCodes.Status401Unauthorized
						? CreateUnauthorizedProblem()
						: CreateForbiddenProblem();

				await context.Response.WriteAsync(JsonSerializer.Serialize(problem));
			}
			catch (InvalidOperationException)
			{
				// Response was modified concurrently, ignore
			}
		}
	}

	private static ProblemDetails CreateUnauthorizedProblem() =>
		new()
		{
			Status = StatusCodes.Status401Unauthorized,
			Title = "Unauthorized",
			Type = "https://httpstatuses.com/401",
			Detail = "Invalid login or password",
		};

	private static ProblemDetails CreateForbiddenProblem() =>
		new()
		{
			Status = StatusCodes.Status403Forbidden,
			Title = "Forbidden",
			Type = "https://httpstatuses.com/403",
			Detail = "Access denied to the requested resource",
		};
}
