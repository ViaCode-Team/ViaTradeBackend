using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ViaTrade.Api.Swagger.Filters;

public sealed class JwtSecurityRequirementsOperationFilter : IOperationFilter
{
	public void Apply(OpenApiOperation operation, OperationFilterContext context)
	{
		var metadata = context.ApiDescription.ActionDescriptor.EndpointMetadata;
		var allowAnonymous = metadata.OfType<IAllowAnonymous>().Any();

		var requiresJwt = !allowAnonymous;

		if (!requiresJwt)
			return;

		var jwtScheme = new OpenApiSecuritySchemeReference(JwtBearerDefaults.AuthenticationScheme, context.Document);

		var components = context.Document.Components ??= new OpenApiComponents();
		components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();

		components.SecuritySchemes[JwtBearerDefaults.AuthenticationScheme] = new OpenApiSecurityScheme
		{
			Description = "Write JWT token without the Bearer prefix.",
			Type = SecuritySchemeType.Http,
			Scheme = "bearer",
			BearerFormat = "JWT",
		};

		var requirement = new OpenApiSecurityRequirement { [jwtScheme] = [] };
		operation.Security = [requirement];
	}
}
