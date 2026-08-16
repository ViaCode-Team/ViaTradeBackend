using System.Text.Json;

namespace ViaTrade.Api.Routing;

public sealed class CamelCaseRouteTokenTransformer : IOutboundParameterTransformer
{
	public string? TransformOutbound(object? value)
	{
		if (value is not string routeToken || string.IsNullOrEmpty(routeToken))
			return null;

		return JsonNamingPolicy.CamelCase.ConvertName(routeToken);
	}
}
