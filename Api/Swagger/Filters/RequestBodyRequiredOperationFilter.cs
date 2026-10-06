using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ViaTrade.Api.Swagger.Filters;

public sealed class RequestBodyRequiredOperationFilter : IOperationFilter
{
	public void Apply(OpenApiOperation operation, OperationFilterContext context)
	{
		if (operation.RequestBody is not OpenApiRequestBody body)
			return;

		body.Required |= context.ApiDescription.ParameterDescriptions.Any(parameter =>
			parameter.Source == BindingSource.Body
			&& (parameter.IsRequired || parameter.ModelMetadata?.IsRequired == true)
		);
	}
}
