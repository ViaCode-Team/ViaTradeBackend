using Swashbuckle.AspNetCore.SwaggerUI;

namespace ViaTrade.Api.Swagger;

public static class SwaggerMiddlewareExtensions
{
	public static IApplicationBuilder UseViaTradeSwagger(this IApplicationBuilder app, IWebHostEnvironment env)
	{
		if (env.IsDevelopment())
		{
			app.UseSwagger();

			app.UseSwaggerUI(options =>
			{
				ConfigureCommonUI(options);
				ConfigureEndpointsUI(options);
			});
		}

		return app;
	}

	private static void ConfigureCommonUI(SwaggerUIOptions options)
	{
		options.RoutePrefix = string.Empty;
		options.ConfigObject.AdditionalItems.Add("withCredentials", true);

		options.EnableTryItOutByDefault();
		options.EnablePersistAuthorization();
		options.DisplayRequestDuration();
		options.DisplayOperationId();
	}

	private static void ConfigureEndpointsUI(SwaggerUIOptions options)
	{
		foreach (var (name, info) in SwaggerDocuments.AllDocuments)
		{
			options.SwaggerEndpoint($"/swagger/{name}/swagger.yaml", $"{info.Title} {info.Version}");
		}
	}
}
