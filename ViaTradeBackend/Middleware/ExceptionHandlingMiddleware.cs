using Application.Common.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace ViaTradeBackend.Middleware;

public class ExceptionHandlingMiddleware(
	IProblemDetailsService problemDetailsService,
	ILogger<ExceptionHandlingMiddleware> logger
) : IExceptionHandler
{
	public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken ct)
	{
		if (context.RequestAborted.IsCancellationRequested)
		{
			logger.LogDebug("Request was aborted by the client: Path={Path}", context.Request.Path);
			return true;
		}

		var descriptor = MapException(exception);
		LogException(context, exception, descriptor.Status);

		if (context.Response.HasStarted)
			return false;

		context.Response.StatusCode = descriptor.Status;

		var problem = new ProblemDetails
		{
			Status = descriptor.Status,
			Title = descriptor.Title,
			Type = $"https://httpstatuses.io/{descriptor.Status}",
			Detail = descriptor.Detail,
			Instance = context.Request.Path,
		};

		problem.Extensions["code"] = descriptor.Code;
		problem.Extensions["traceId"] = context.TraceIdentifier;

		if (exception is ValidationException validationException)
			problem.Extensions["errors"] = validationException.Errors;

		return await problemDetailsService.TryWriteAsync(
			new ProblemDetailsContext { HttpContext = context, ProblemDetails = problem }
		);
	}

	private static ErrorDescriptor MapException(Exception exception)
	{
		return exception switch
		{
			ValidationException ex => new(400, "Validation Failed", ex.Code, ex.Message),
			BadRequestException ex => new(400, "Bad Request", ex.Code, ex.Message),
			InvalidCredentialsException ex => new(401, "Unauthorized", ex.Code, ex.Message),
			InvalidTokenException ex => new(401, "Unauthorized", ex.Code, ex.Message),
			AuthenticationException ex => new(401, "Unauthorized", ex.Code, ex.Message),
			ForbiddenException ex => new(403, "Forbidden", ex.Code, ex.Message),
			NotFoundException ex => new(404, "Not Found", ex.Code, ex.Message),
			ConflictException ex => new(409, "Conflict", ex.Code, ex.Message),
			BusinessRuleException ex => new(422, "Business Rule Violation", ex.Code, ex.Message),
			ServiceUnavailableException ex => new(503, "Service Unavailable", ex.Code, ex.Message),
			DataIntegrityException ex => new(500, "Internal Server Error", ex.Code, "Server data is inconsistent."),
			ArgumentException ex => new(400, "Bad Request", "invalid_argument", ex.Message),
			KeyNotFoundException => new(404, "Not Found", "not_found", "The requested resource was not found."),
			UnauthorizedAccessException => new(401, "Unauthorized", "unauthorized", "Authentication is required."),
			OperationCanceledException => new(408, "Request Timeout", "request_timeout", "The operation timed out."),
			_ => new(500, "Internal Server Error", "internal_error", "An unexpected server error occurred."),
		};
	}

	private void LogException(HttpContext context, Exception exception, int status)
	{
		if (status >= 500)
		{
			logger.LogError(
				exception,
				"Unhandled exception: Status={Status}, Path={Path}, TraceId={TraceId}",
				status,
				context.Request.Path,
				context.TraceIdentifier
			);
			return;
		}

		logger.LogInformation(
			"Expected exception handled: Status={Status}, Path={Path}, Type={ExceptionType}, TraceId={TraceId}",
			status,
			context.Request.Path,
			exception.GetType().Name,
			context.TraceIdentifier
		);
	}

	private sealed record ErrorDescriptor(int Status, string Title, string Code, string Detail);
}
