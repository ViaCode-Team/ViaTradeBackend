using System.Reflection;
using MicroElements.OpenApi.FluentValidation;
using MicroElements.Swashbuckle.FluentValidation;
using MicroElements.Swashbuckle.FluentValidation.AspNetCore;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;
using ViaTrade.Api.Swagger.Filters;

namespace ViaTrade.Api.Swagger;

public static class SwaggerServiceExtensions
{
	public static IServiceCollection AddViaTradeSwagger(this IServiceCollection services)
	{
		services.AddFluentValidationRulesToSwagger(options => options.RemoveUnusedQuerySchemas = false);
		services.AddSingleton(provider =>
			FloatingPointRangeRule.Create(provider.GetRequiredService<IOptions<SchemaGenerationOptions>>())
		);

		services.AddSwaggerGen(options =>
		{
			ConfigureNullableReferenceTypes(options);

			ConfigureFilters(options);

			ConfigureOperationIds(options);

			ConfigureXmlComments(options);

			ConfigureDocuments(options);
		});

		services.PostConfigure<SwaggerGenOptions>(options =>
			options.OperationFilterDescriptors.RemoveAll(filter =>
				filter.Type == typeof(FluentValidationOperationFilterScopeAdapter)
			)
		);

		return services;
	}

	private static void ConfigureNullableReferenceTypes(SwaggerGenOptions options)
	{
		options.SupportNonNullableReferenceTypes();
		options.NonNullableReferenceTypesAsRequired();
		options.SchemaFilter<OptionalPropertiesAsNonNullableSchemaFilter>();
	}

	private static void ConfigureFilters(SwaggerGenOptions options)
	{
		options.DocumentFilter<ProblemDetailsDocumentFilter>();
		options.OperationFilter<ProblemDetailsOperationFilter>();
		options.OperationFilter<CamelCaseParameterFilter>();
		options.OperationFilter<IgnorePropertiesOperationFilter>();

		options.OperationFilter<RequestPropertiesOperationFilter>();
		options.OperationFilter<RequestBodyRequiredOperationFilter>();
		options.OperationFilter<AuthCookiesOperationFilter>();
		options.OperationFilter<JwtSecurityRequirementsOperationFilter>();

		options.OperationFilter<ServicePasswordSecurityRequirementsOperationFilter>();
	}

	private static void ConfigureOperationIds(SwaggerGenOptions options)
	{
		options.CustomOperationIds(apiDesc =>
		{
			var hasMethodInfo = apiDesc.TryGetMethodInfo(out var methodInfo);
			if (hasMethodInfo)
			{
				return methodInfo.Name;
			}
			return null;
		});
	}

	private static void ConfigureXmlComments(SwaggerGenOptions options)
	{
		var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
		var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
		if (File.Exists(xmlPath))
			options.IncludeXmlComments(xmlPath);
	}

	private static void ConfigureDocuments(SwaggerGenOptions options)
	{
		foreach (var (name, info) in SwaggerDocuments.AllDocuments)
		{
			options.SwaggerDoc(name, info);
		}

		options.DocInclusionPredicate(
			(documentName, apiDesc) =>
			{
				if (documentName == SwaggerDocuments.Web)
					return string.IsNullOrEmpty(apiDesc.GroupName)
						|| string.Equals(apiDesc.GroupName, documentName, StringComparison.OrdinalIgnoreCase);

				return string.Equals(apiDesc.GroupName, documentName, StringComparison.OrdinalIgnoreCase);
			}
		);
	}
}
