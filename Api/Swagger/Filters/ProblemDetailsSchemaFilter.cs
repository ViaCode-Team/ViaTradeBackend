using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ViaTrade.Api.Swagger.Filters;

public class ProblemDetailsSchemaFilter : ISchemaFilter
{
	public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
	{
		if (context.Type != typeof(ProblemDetails) || schema is not OpenApiSchema openApiSchema)
			return;

		openApiSchema.Required = new HashSet<string>
		{
			"type",
			"title",
			"status",
			"detail",
			"instance",
			"code",
			"traceId",
		};
	}
}
