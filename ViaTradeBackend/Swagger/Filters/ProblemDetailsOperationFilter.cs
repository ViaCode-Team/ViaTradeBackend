using System.Text.Json.Nodes;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ViaTradeBackend.Swagger.Filters;

public class ProblemDetailsOperationFilter : IOperationFilter
{
	public void Apply(OpenApiOperation operation, OperationFilterContext context)
	{
		var errorResponses = new Dictionary<int, string>
		{
			[400] = "Bad Request - Invalid input data",
			[401] = "Unauthorized - Authentication is required",
			[403] = "Forbidden - Access denied",
			[404] = "Not Found - Resource does not exist",
			[408] = "Request Timeout - Operation timed out",
			[409] = "Conflict - Resource state conflict",
			[422] = "Unprocessable Content - Business rule violation",
			[500] = "Internal Server Error - Unexpected error",
			[503] = "Service Unavailable - Temporary infrastructure issue",
		};

		foreach (var (statusCode, description) in errorResponses)
		{
			operation.Responses?.TryAdd(
				statusCode.ToString(),
				new OpenApiResponse
				{
					Description = description,
					Content = new Dictionary<string, OpenApiMediaType>
					{
						["application/problem+json"] = new OpenApiMediaType
						{
							Schema = new OpenApiSchemaReference("ProblemDetails", context.Document),
							Example = CreateExample(statusCode, description),
						},
					},
				}
			);
		}
	}

	private static JsonObject CreateExample(int statusCode, string description)
	{
		string title = description.Split(" - ")[0];
		return new JsonObject
		{
			["type"] = $"https://httpstatuses.io/{statusCode}",
			["title"] = title,
			["status"] = statusCode,
			["detail"] = "Safe error description.",
			["instance"] = "/api/resource",
			["code"] = "error_code",
			["traceId"] = "0HNABC123:00000001",
		};
	}
}
