using Application.Strategies.Models;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ViaTradeBackend.Swagger.Filters;

public sealed class StrategyInstrumentFilterOperationFilter : IOperationFilter
{
	public void Apply(OpenApiOperation operation, OperationFilterContext context)
	{
		var actionName = context.ApiDescription.ActionDescriptor.RouteValues["action"];
		if (!string.Equals(actionName, "GetInstrumentsByStrategy", StringComparison.Ordinal))
			return;

		var parameter = operation.Parameters?.OfType<OpenApiParameter>().FirstOrDefault(parameter =>
			parameter.In == ParameterLocation.Query
			&& string.Equals(parameter.Name, "instrumentIds", StringComparison.OrdinalIgnoreCase)
		);

		if (parameter?.Schema is not OpenApiSchema schema || schema.Items is not OpenApiSchema items)
			return;

		schema.MaxItems = StrategyInstrumentFilter.MaxInstrumentIds;
		items.Minimum = "1";
		parameter.Style = ParameterStyle.Form;
		parameter.Explode = true;
	}
}
