using System.Reflection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using ViaTradeBackend.Swagger.Filters;

namespace ViaTradeBackend.Swagger;

public static class SwaggerServiceExtensions
{
	public static IServiceCollection AddViaTradeSwagger(this IServiceCollection services)
	{
		services.AddEndpointsApiExplorer();

		services.AddSwaggerGen(options =>
		{
			options.SwaggerDoc(
				"v1",
				new OpenApiInfo
				{
					Title = "ViaTrade API",
					Version = "v1",
					Description = "API для платформы инвестиционного анализа ViaTrade",
				}
			);

			options.SwaggerDoc(
				"internal",
				new OpenApiInfo
				{
					Title = "ViaTrade Internal API",
					Version = "v1",
					Description = "Сервисные endpoints для внутренних интеграций",
				}
			);

			options.SupportNonNullableReferenceTypes();
			options.NonNullableReferenceTypesAsRequired();
			options.SchemaFilter<OptionalPropertiesAsNonNullableSchemaFilter>();

			options.CustomOperationIds(apiDesc =>
			{
				if (apiDesc.TryGetMethodInfo(out var methodInfo))
				{
					return methodInfo.Name;
				}
				return null;
			});

			options.DocInclusionPredicate(
				(documentName, apiDesc) =>
				{
					if (documentName == "internal")
						return apiDesc.GroupName == "internal";

					return apiDesc.GroupName != "internal";
				}
			);

			options.TagActionsBy(apiDesc =>
			{
				if (apiDesc.GroupName == "internal")
					return ["Internal"];

				return [apiDesc.ActionDescriptor.RouteValues["controller"] ?? "Default"];
			});

			options.DocumentFilter<ProblemDetailsDocumentFilter>();

			options.AddSecurityDefinition(
				JwtBearerDefaults.AuthenticationScheme,
				new OpenApiSecurityScheme
				{
					Description = "Write JWT token without the Bearer prefix.",
					Type = SecuritySchemeType.Http,
					Scheme = "bearer",
					BearerFormat = "JWT",
				}
			);

			options.AddSecurityDefinition(
				"ServicePassword",
				new OpenApiSecurityScheme
				{
					Description = "Write service password.",
					Name = "TgBot-Service-Password",
					In = ParameterLocation.Header,
					Type = SecuritySchemeType.ApiKey,
				}
			);

			options.OperationFilter<ProblemDetailsOperationFilter>();
			options.OperationFilter<CamelCaseParameterFilter>();
			options.OperationFilter<SecurityRequirementsOperationFilter>();

			var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
			var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
			if (File.Exists(xmlPath))
				options.IncludeXmlComments(xmlPath);
		});

		return services;
	}
}
