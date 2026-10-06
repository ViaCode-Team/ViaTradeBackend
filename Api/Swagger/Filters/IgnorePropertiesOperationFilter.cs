using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using ViaTrade.Api.Attributes.Binding;
using ViaTrade.Api.ModelBinding;

namespace ViaTrade.Api.Swagger.Filters;

public sealed class IgnorePropertiesOperationFilter(IOptions<JsonOptions> jsonOptions) : IOperationFilter
{
	public void Apply(OpenApiOperation operation, OperationFilterContext context)
	{
		var parameter = context
			.MethodInfo.GetParameters()
			.SingleOrDefault(parameter =>
				(
					parameter.GetCustomAttribute<IgnorePropertiesAttribute>() is not null
					|| parameter.GetCustomAttributes<FromRequestPropertiesAttribute>().Any()
				) && RequestPropertyBinding.From(parameter, context.ApiDescription.ActionDescriptor).HasBody
			);

		if (parameter is null || operation.RequestBody?.Content is null)
			return;

		var ignoredNames = RequestPropertyBinding
			.From(parameter, context.ApiDescription.ActionDescriptor)
			.GetJsonBodyExclusions(jsonOptions.Value.JsonSerializerOptions);

		foreach (var mediaType in operation.RequestBody.Content.Values)
		{
			var source = mediaType.Schema;
			if (source is OpenApiSchemaReference reference)
			{
				var referenceId = reference.Reference.Id;
				if (referenceId is null)
					continue;

				var foundSchema = context.SchemaRepository.Schemas.TryGetValue(referenceId, out var resolvedSchema);
				if (!foundSchema)
					continue;

				source = resolvedSchema;
			}

			if (source is not OpenApiSchema schema)
				continue;

			var filtered = (OpenApiSchema)schema.CreateShallowCopy();
			filtered.Properties = schema
				.Properties?.Where(property => !ignoredNames.Contains(property.Key))
				.ToDictionary(property => property.Key, property => property.Value);
			filtered.Required = schema.Required?.Where(property => !ignoredNames.Contains(property)).ToHashSet();
			mediaType.Schema = filtered;
		}
	}
}
