using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ViaTradeBackend.Swagger.Filters;

public sealed class OptionalPropertiesAsNonNullableSchemaFilter : ISchemaFilter
{
	public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
	{
		if (schema.Properties is null || schema.Properties.Count == 0)
			return;

		foreach (var property in schema.Properties.ToList())
		{
			if (schema.Required.Contains(property.Key))
				continue;

			if (property.Value is not OpenApiSchema openApiSchema)
				continue;

			if (openApiSchema.Type is not { } type)
				continue;

			if (!type.HasFlag(JsonSchemaType.Null))
				continue;

			var copiedSchema = (OpenApiSchema)openApiSchema.CreateShallowCopy();

			copiedSchema.Type = type & ~JsonSchemaType.Null;

			schema.Properties[property.Key] = copiedSchema;
		}
	}
}