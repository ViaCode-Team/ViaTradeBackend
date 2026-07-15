using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Text.Json.Nodes;

namespace ViaTradeBackend.Swagger.Filters;

public class ProblemDetailsDocumentFilter : IDocumentFilter
{
	public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
	{
		swaggerDoc.Components ??= new();

		swaggerDoc.Components?.Schemas?["ProblemDetails"] = new OpenApiSchema
		{
			Type = JsonSchemaType.Object,
			Required = new HashSet<string> { "type", "status", "title", "detail" },
			Properties = new Dictionary<string, IOpenApiSchema>
			{
				["type"] = new OpenApiSchema { Type = JsonSchemaType.String, Format = "uri" },
				["title"] = new OpenApiSchema { Type = JsonSchemaType.String },
				["status"] = new OpenApiSchema { Type = JsonSchemaType.Integer, Format = "int32" },
				["detail"] = new OpenApiSchema { Type = JsonSchemaType.String },
			},
			Example = new JsonObject
			{
				["type"] = "https://httpstatuses.com/400",
				["title"] = "Bad Request",
				["status"] = 400,
				["detail"] = "Invalid input parameter",
			}
		};
	}
}
