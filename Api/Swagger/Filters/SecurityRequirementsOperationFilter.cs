using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using ViaTrade.Api.Attribute;

namespace ViaTrade.Api.Swagger.Filters;

public sealed class SecurityRequirementsOperationFilter : IOperationFilter
{
	private const string ServicePasswordScheme = "ServicePassword";

	public void Apply(OpenApiOperation operation, OperationFilterContext context)
	{
		var metadata = context.ApiDescription.ActionDescriptor.EndpointMetadata;

		var allowAnonymous = metadata.OfType<IAllowAnonymous>().Any();

		var hasServicePassword = metadata.OfType<ServicePasswordAttribute>().Any();

		// FallbackPolicy default is [Authorize]
		var requiresJwt = !allowAnonymous;
		var requiresServicePassword = hasServicePassword;

		if (!requiresJwt && !requiresServicePassword)
			return;

		var requirement = new OpenApiSecurityRequirement();

		if (requiresJwt)
		{
			var jwtScheme = new OpenApiSecuritySchemeReference(
				JwtBearerDefaults.AuthenticationScheme,
				context.Document
			);

			requirement[jwtScheme] = [];
		}

		if (requiresServicePassword)
		{
			var servicePasswordScheme = new OpenApiSecuritySchemeReference(ServicePasswordScheme, context.Document);

			requirement[servicePasswordScheme] = [];
		}

		operation.Security = [requirement];
	}
}
