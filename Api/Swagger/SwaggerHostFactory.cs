using ViaTrade.Application;
using ViaTrade.Configuration;

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

		builder.Services.AddConfigurationLayer(builder.Configuration).AddApiMvc().AddValidation().AddViaTradeSwagger();

		var app = builder.Build();

		app.MapControllers();

		return app;
	}
}
