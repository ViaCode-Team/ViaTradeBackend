using System.Reflection;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using ViaTrade.Api.Attributes.Binding;
using ViaTrade.Api.ModelBinding;

namespace ViaTrade.Api.Swagger.Filters;

public sealed class RequestPropertiesOperationFilter(
	IOptions<JsonOptions> jsonOptions,
	IModelMetadataProvider metadataProvider
) : IOperationFilter
{
	public void Apply(OpenApiOperation operation, OperationFilterContext context)
	{
		var parameters = context
			.MethodInfo.GetParameters()
			.Where(parameter => ShouldProject(parameter, context))
			.ToList();
		var formContentType = "application/x-www-form-urlencoded";
		if (
			parameters.Any(parameter =>
				RequestPropertyBinding.From(parameter, context.ApiDescription.ActionDescriptor).HasFormFiles
			)
		)
			formContentType = "multipart/form-data";
		if (
			parameters.Any(parameter =>
				RequestPropertyBinding.From(parameter, context.ApiDescription.ActionDescriptor).HasForm
			)
		)
			operation.RequestBody = new OpenApiRequestBody { Content = new Dictionary<string, OpenApiMediaType>() };

		foreach (var parameter in parameters)
			ProjectParameter(operation, context, parameter, formContentType);
	}

	private bool ShouldProject(ParameterInfo parameter, OperationFilterContext context)
	{
		var metadata = ((ModelMetadataProvider)metadataProvider).GetMetadataForParameter(parameter);
		var source = context
			.ApiDescription.ActionDescriptor.Parameters.Single(descriptor => descriptor.Name == parameter.Name)
			.BindingInfo?.BindingSource;
		if (metadata.BinderType is not null || source == BindingSource.Services)
			return false;
		return parameter.GetCustomAttributes<FromRequestPropertiesAttribute>().Any()
			|| parameter.GetCustomAttribute<FromFormAttribute>() is not null
			|| RequestPropertyBinding.From(metadata, source).RequiresCustomBinding(metadata, source);
	}

	private void ProjectParameter(
		OpenApiOperation operation,
		OperationFilterContext context,
		ParameterInfo parameter,
		string formContentType
	)
	{
		var schema = Resolve(
			context.SchemaGenerator.GenerateSchema(parameter.ParameterType, context.SchemaRepository),
			context
		);
		var metadata = ((ModelMetadataProvider)metadataProvider).GetMetadataForParameter(parameter);
		var configuration = RequestPropertyBinding.From(parameter, context.ApiDescription.ActionDescriptor);
		configuration.ValidateClientNames(metadata);
		operation.Parameters ??= [];
		if (!metadata.IsComplexType || metadata.IsEnumerableType)
		{
			AddField(
				operation,
				metadata.BinderModelName ?? parameter.Name!,
				schema,
				metadata.IsRequired,
				BindingSource.Form,
				formContentType
			);
			return;
		}
		var requestParameters = context
			.ApiDescription.ParameterDescriptions.Where(description =>
				description.ParameterDescriptor?.Name == parameter.Name
			)
			.Select(description => description.Name)
			.ToHashSet(StringComparer.OrdinalIgnoreCase);
		operation.Parameters = operation
			.Parameters.Where(parameter =>
				parameter.In == ParameterLocation.Path || !requestParameters.Contains(parameter.Name!)
			)
			.ToList();

		foreach (var member in RequestPropertyBinding.GetMembers(metadata))
		{
			if (!configuration.IsBindingAllowed(metadata, member))
				continue;
			var property = parameter.ParameterType.GetProperty(RequestPropertyBinding.GetPropertyName(member))!;
			var jsonName = RequestPropertyBinding.GetJsonName(property, jsonOptions.Value.JsonSerializerOptions);
			IOpenApiSchema? propertySchema = null;
			schema.Properties?.TryGetValue(jsonName, out propertySchema);
			if (propertySchema is null)
				continue;
			var name = GetParameterName(configuration, member);
			var source = configuration.GetSource(member);
			if (source == BindingSource.Path)
				name =
					context
						.ApiDescription.ParameterDescriptions.FirstOrDefault(description =>
							description.Source == BindingSource.Path
							&& string.Equals(description.Name, name, StringComparison.OrdinalIgnoreCase)
						)
						?.Name
					?? name;

			if (source is null || source == BindingSource.Body)
				continue;
			var resolved = Resolve(propertySchema!, context);
			if (
				resolved.Type?.HasFlag(JsonSchemaType.Object) == true
				&& (source == BindingSource.Query || source == BindingSource.Form)
			)
				ProjectNestedFields(
					operation,
					configuration,
					member,
					resolved,
					schema.Required?.Contains(jsonName) == true,
					formContentType
				);
			else
				AddField(
					operation,
					name,
					propertySchema!,
					schema.Required?.Contains(jsonName) == true || member.IsBindingRequired,
					source,
					formContentType
				);
		}
	}

	private void ProjectNestedFields(
		OpenApiOperation operation,
		RequestPropertyBinding configuration,
		ModelMetadata parent,
		IOpenApiSchema schema,
		bool parentRequired,
		string formContentType
	)
	{
		foreach (var member in RequestPropertyBinding.GetMembers(parent))
		{
			if (!member.IsBindingAllowed || parent.PropertyFilterProvider?.PropertyFilter(member) == false)
				continue;
			var property = parent.ModelType.GetProperty(RequestPropertyBinding.GetPropertyName(member))!;
			var jsonName = RequestPropertyBinding.GetJsonName(property, jsonOptions.Value.JsonSerializerOptions);
			IOpenApiSchema? child = null;
			var found = schema.Properties?.TryGetValue(jsonName, out child) == true;
			if (!found)
				continue;
			var name = member.BinderModelName ?? JsonNamingPolicy.CamelCase.ConvertName(property.Name);
			if (!configuration.IsFlattened(parent))
				name = ModelNames.CreatePropertyModelName(GetParameterName(configuration, parent), name);
			var required =
				parentRequired
				&& schema.Required?.Contains(jsonName) == true
				&& child!.Default is null
				&& (child.Type?.HasFlag(JsonSchemaType.Array) != true || child.MinItems > 0);
			AddField(
				operation,
				name,
				child!,
				required || member.IsBindingRequired,
				configuration.GetSource(parent)!,
				formContentType
			);
		}
	}

	private static string GetParameterName(RequestPropertyBinding configuration, ModelMetadata member)
	{
		var name = configuration.GetName(member);
		if (
			configuration.HasAlias(RequestPropertyBinding.GetPropertyName(member)) || member.BinderModelName is not null
		)
			return name;
		return JsonNamingPolicy.CamelCase.ConvertName(name);
	}

	private static void AddField(
		OpenApiOperation operation,
		string name,
		IOpenApiSchema schema,
		bool required,
		BindingSource source,
		string formContentType
	)
	{
		if (source == BindingSource.Form || source == BindingSource.FormFile)
		{
			var body = (OpenApiRequestBody)operation.RequestBody!;
			var contentType = formContentType;
			if (!body.Content!.ContainsKey(contentType))
				body.Content[contentType] = new OpenApiMediaType
				{
					Schema = new OpenApiSchema
					{
						Type = JsonSchemaType.Object,
						Properties = new Dictionary<string, IOpenApiSchema>(),
						Required = new HashSet<string>(),
					},
				};
			var form = (OpenApiSchema)body.Content[contentType].Schema!;
			form.Properties![name] = schema;
			if (required)
				form.Required!.Add(name);
			body.Required |= required;
			return;
		}

		var location = source.Id switch
		{
			var id when id == BindingSource.Path.Id => ParameterLocation.Path,
			var id when id == BindingSource.Header.Id => ParameterLocation.Header,
			var id when id == BindingSource.Query.Id => ParameterLocation.Query,
			var id when id == BindingSource.ModelBinding.Id => ParameterLocation.Query,
			_ => throw new ArgumentOutOfRangeException(nameof(source)),
		};
		var existing = operation
			.Parameters!.OfType<OpenApiParameter>()
			.SingleOrDefault(parameter =>
				parameter.In == location && string.Equals(parameter.Name, name, StringComparison.OrdinalIgnoreCase)
			);
		if (existing is not null)
		{
			if (source != BindingSource.Path)
				throw new InvalidOperationException($"Duplicate {source} parameter '{name}'.");
			existing.Schema = schema;
			existing.Name = name;
			existing.Required = true;
			return;
		}
		var projected = new OpenApiParameter
		{
			Name = name,
			In = location,
			Schema = schema,
			Required = required || source == BindingSource.Path,
			Style = ParameterStyle.Form,
			Explode = true,
		};
		if (source == BindingSource.Path || source == BindingSource.Header)
			projected.Style = ParameterStyle.Simple;
		operation.Parameters!.Add(projected);
	}

	private static IOpenApiSchema Resolve(IOpenApiSchema schema, OperationFilterContext context)
	{
		if (schema is OpenApiSchemaReference reference && reference.Reference.Id is { } id)
			return context.SchemaRepository.Schemas[id];
		return schema;
	}
}
