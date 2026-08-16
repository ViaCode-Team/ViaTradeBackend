using System.Text.Json.Nodes;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ViaTrade.Api.Swagger.Filters;

public class ProblemDetailsDocumentFilter : IDocumentFilter
{
	public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
	{
		var components = swaggerDoc.Components ??= new OpenApiComponents();

		components.Schemas!["ProblemDetails"] = new OpenApiSchema
		{
			Type = JsonSchemaType.Object,
			Required = new HashSet<string> { "type", "title", "status", "detail", "instance", "code", "traceId" },
			Properties = CreateProperties(),
			Example = CreateExample(),
		};
	}

	private static Dictionary<string, IOpenApiSchema> CreateProperties()
	{
		return new Dictionary<string, IOpenApiSchema>
		{
			["type"] = new OpenApiSchema { Type = JsonSchemaType.String, Format = "uri" },
			["title"] = new OpenApiSchema { Type = JsonSchemaType.String },
			["status"] = new OpenApiSchema { Type = JsonSchemaType.Integer, Format = "int32" },
			["detail"] = new OpenApiSchema { Type = JsonSchemaType.String },
			["instance"] = new OpenApiSchema { Type = JsonSchemaType.String },
			["code"] = new OpenApiSchema { Type = JsonSchemaType.String },
			["traceId"] = new OpenApiSchema { Type = JsonSchemaType.String },
			["errors"] = new OpenApiSchema
			{
				Type = JsonSchemaType.Object,
				AdditionalPropertiesAllowed = true,
				AdditionalProperties = new OpenApiSchema
				{
					Type = JsonSchemaType.Array,
					Items = new OpenApiSchema { Type = JsonSchemaType.String },
				},
			},
		};
	}

	private static JsonObject CreateExample()
	{
		return new JsonObject
		{
			["type"] = "https://httpstatuses.io/400",
			["title"] = "Validation Failed",
			["status"] = 400,
			["detail"] = "One or more validation errors occurred.",
			["instance"] = "/api/trades",
			["code"] = "validation_failed",
			["traceId"] = "0HNABC123:00000001",
		};
	}
}
