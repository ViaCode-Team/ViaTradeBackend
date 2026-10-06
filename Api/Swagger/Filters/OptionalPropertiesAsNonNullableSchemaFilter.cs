using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ViaTrade.Api.Swagger.Filters;

public sealed class OptionalPropertiesAsNonNullableSchemaFilter(IActionDescriptorCollectionProvider actionDescriptors)
	: ISchemaFilter
{
	public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
	{
		if (
			actionDescriptors.ActionDescriptors.Items.Any(action =>
				action.Parameters.Any(parameter => parameter.ParameterType == context.Type)
			)
		)
			return;

		if (schema.Properties is null || schema.Properties.Count == 0)
			return;

		var requiredProperties = schema.Required;

		foreach (var (propertyName, propertySchema) in schema.Properties.ToList())
		{
			if (requiredProperties?.Contains(propertyName) == true)
				continue;

			var type = propertySchema.Type;

			if (type is null || !type.Value.HasFlag(JsonSchemaType.Null))
				continue;

			var copiedSchema = (OpenApiSchema)propertySchema.CreateShallowCopy();
			copiedSchema.Type = type.Value & ~JsonSchemaType.Null;

			schema.Properties[propertyName] = copiedSchema;
		}
	}
}
