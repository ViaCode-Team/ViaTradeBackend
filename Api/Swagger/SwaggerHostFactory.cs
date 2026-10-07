using ViaTrade.Application;
using ViaTrade.Configuration;
using ViaTrade.Infrastructure;

namespace ViaTrade.Api.Swagger;

public static class SwaggerHostFactory
{
	public static IHost CreateHost()
	{
		var builder = WebApplication.CreateBuilder(
			new WebApplicationOptions
			{
				ApplicationName = typeof(SwaggerHostFactory).Assembly.FullName,
				EnvironmentName = Environments.Development,
			}
		);

		builder
			.Services.AddConfigurationLayer(builder.Configuration)
			.AddApplicationLayer()
			.AddInfrastructureLayer(builder.Configuration)
			.AddApiLayer();

		builder.Services.AddApiDocumentation();

		var app = builder.Build();

		app.MapControllers();

		return app;
	}
}
