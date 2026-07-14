using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Text.Json.Nodes;

namespace ViaTradeBackend.Swagger.Filters;

public class ProblemDetailsOperationFilter : IOperationFilter
{
	public void Apply(OpenApiOperation operation, OperationFilterContext context)
	{
		var problemDetailsSchema = context.SchemaGenerator.GenerateSchema(
			typeof(ProblemDetails),
			context.SchemaRepository
		);

		var errorResponses = new Dictionary<int, string>
		{
			{ 400, "Bad Request - Invalid input data" },
			{ 401, "Unauthorized - Invalid credentials" },
			{ 403, "Forbidden - Access denied" },
			{ 404, "Not Found - Resource does not exist" },
			{ 408, "Timeout - Server timeout" },
			{ 409, "Conflict - Conflict operation" },
			{ 500, "Internal Server Error - Unexpected error" }
		};

		foreach (var (statusCode, description) in errorResponses)
		{
			operation.Responses?.TryAdd(statusCode.ToString(), new OpenApiResponse
			{
				Description = description,
				Content = new Dictionary<string, OpenApiMediaType>
				{
					["application/problem+json"] = new OpenApiMediaType
					{
						Schema = problemDetailsSchema,
						Example = new JsonObject
						{
							["type"] = $"https://httpstatuses.com/{statusCode}",
							["title"] = description.Split(" - ")[0],
							["status"] = statusCode,
							["detail"] = "Specific error details here"
						}
					}
				}
			});
		}
	}
}