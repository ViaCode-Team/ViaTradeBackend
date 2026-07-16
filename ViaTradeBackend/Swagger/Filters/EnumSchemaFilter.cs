using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Text.Json.Nodes;

namespace ViaTradeBackend.Swagger.Filters;

public sealed class EnumSchemaFilter : ISchemaFilter
{
	public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
	{
		var enumType = Nullable.GetUnderlyingType(context.Type) ?? context.Type;

		if (!enumType.IsEnum)
			return;

		var extensions = EnsureExtensions(schema);

		var names = new JsonArray();

		foreach (var name in Enum.GetNames(enumType))
			names.Add(name);

		extensions["x-enumNames"] = new JsonNodeExtension(names);
	}

	private static IDictionary<string, IOpenApiExtension> EnsureExtensions(IOpenApiSchema schema)
	{
		if (schema.Extensions is not null)
			return schema.Extensions;

		var property = schema.GetType().GetProperty(nameof(schema.Extensions));

		var dictionary = new Dictionary<string, IOpenApiExtension>();

		property?.SetValue(schema, dictionary);

		return dictionary;
	}
}
