using Microsoft.OpenApi;
using ViaTrade.Api.Routing;

namespace ViaTrade.Api.Swagger;

public static class SwaggerDocuments
{
	public const string Web = "web";

	public static readonly IReadOnlyDictionary<string, OpenApiInfo> AllDocuments = new Dictionary<string, OpenApiInfo>
	{
		[Web] = new OpenApiInfo
		{
			Title = "ViaTrade Web API",
			Version = "v1",
			Description = "API для платформы инвестиционного анализа ViaTrade",
		},
		[InternalServices.TgBot] = new OpenApiInfo
		{
			Title = "ViaTrade TgBot API",
			Version = "v1",
			Description = "Сервисные endpoints для интеграции с TgBot",
		},
		[InternalServices.Analyzer] = new OpenApiInfo
		{
			Title = "ViaTrade Analyzer API",
			Version = "v1",
			Description = "Сервисные endpoints для интеграции с Analyzer",
		},
	};
}
