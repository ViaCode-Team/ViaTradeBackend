using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;
using ViaTradeBackend.Swagger.Filters;

namespace ViaTradeBackend.Swagger;

public static class SwaggerServiceExtensions
{
	public static IServiceCollection AddViaTradeSwagger(this IServiceCollection services)
	{
		services.AddEndpointsApiExplorer();

		services.AddSwaggerGen(options =>
		{
			options.SwaggerDoc("v1", new OpenApiInfo
			{
				Title = "ViaTrade API",
				Version = "v1",
				Description = "API для платформы инвестиционного анализа ViaTrade"
			});

			options.SupportNonNullableReferenceTypes();
			options.NonNullableReferenceTypesAsRequired();
			options.SchemaFilter<OptionalPropertiesAsNonNullableSchemaFilter>();

			options.CustomOperationIds(apiDesc =>
				apiDesc.TryGetMethodInfo(out var methodInfo) ? methodInfo.Name : null);

			options.DocumentFilter<ProblemDetailsDocumentFilter>();

			options.OperationFilter<ProblemDetailsOperationFilter>();
			options.OperationFilter<CamelCaseParameterFilter>();

			var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
			var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
			if (File.Exists(xmlPath))
				options.IncludeXmlComments(xmlPath);
		});

		return services;
	}
}
