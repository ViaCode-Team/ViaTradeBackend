using Microsoft.AspNetCore.Mvc;

namespace ViaTradeBackend.Middleware;

public class ProblemDetailsStatusCodeMiddleware(
	RequestDelegate next,
	IProblemDetailsService problemDetailsService,
	ILogger<ProblemDetailsStatusCodeMiddleware> logger
)
{
	public async Task Invoke(HttpContext context)
	{
		await next(context);

		if (context.Response.StatusCode is not (StatusCodes.Status401Unauthorized or StatusCodes.Status403Forbidden))
			return;

		if (context.Response.HasStarted || context.Response.ContentLength > 0)
			return;

		string title;
		string code;
		string detail;

		if (context.Response.StatusCode == StatusCodes.Status401Unauthorized)
		{
			title = "Unauthorized";
			code = "unauthorized";
			detail = "Authentication is required.";
		}
		else
		{
			title = "Forbidden";
			code = "forbidden";
			detail = "Access to the requested resource is forbidden.";
		}

		var problem = new ProblemDetails
		{
			Status = context.Response.StatusCode,
			Title = title,
			Type = $"https://httpstatuses.io/{context.Response.StatusCode}",
			Detail = detail,
			Instance = context.Request.Path,
		};

		problem.Extensions["code"] = code;
		problem.Extensions["traceId"] = context.TraceIdentifier;

		bool written = await problemDetailsService.TryWriteAsync(
			new ProblemDetailsContext { HttpContext = context, ProblemDetails = problem }
		);

		if (!written)
			logger.LogWarning("Unable to write authorization ProblemDetails: Path={Path}", context.Request.Path);
	}
}
