using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using ViaTrade.Api.Attribute;

namespace ViaTrade.Api.Swagger.Filters;

public sealed class ServicePasswordSecurityRequirementsOperationFilter : IOperationFilter
{
	private const string ServicePasswordScheme = "ServicePassword";

	public void Apply(OpenApiOperation operation, OperationFilterContext context)
	{
		var metadata = context.ApiDescription.ActionDescriptor.EndpointMetadata;
		var hasServicePassword = metadata.OfType<ServicePasswordAttribute>().Any();

		if (!hasServicePassword)
			return;

		var servicePasswordScheme = new OpenApiSecuritySchemeReference(ServicePasswordScheme, context.Document);

		var components = context.Document.Components ??= new OpenApiComponents();
		components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();

		components.SecuritySchemes[ServicePasswordScheme] = new OpenApiSecurityScheme
		{
			Description = "Write service password.",
			Name = "Service-Password",
			In = ParameterLocation.Header,
			Type = SecuritySchemeType.ApiKey,
		};

		var requirement = new OpenApiSecurityRequirement { [servicePasswordScheme] = [] };

		operation.Security = [requirement];
	}
}
