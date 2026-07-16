using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Text.Json;

namespace ViaTradeBackend.Swagger.Filters;

public class CamelCaseParameterFilter : IOperationFilter
{
	public void Apply(OpenApiOperation operation, OperationFilterContext context)
	{
		if (operation.Parameters == null)
			return;

		foreach (var parameter in operation.Parameters)
		{
			if (parameter is not OpenApiParameter openApiParameter)
				continue;

			var apiParameter = context.ApiDescription.ParameterDescriptions
				.FirstOrDefault(p => string.Equals(p.Name, parameter.Name, StringComparison.OrdinalIgnoreCase));

			bool hasExplicitName = apiParameter?.ModelMetadata?.BinderModelName != null;

			if (!hasExplicitName && !string.IsNullOrEmpty(openApiParameter.Name))
				openApiParameter.Name = JsonNamingPolicy.CamelCase.ConvertName(openApiParameter.Name) ?? openApiParameter.Name;
		}
	}
}
