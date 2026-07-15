namespace ViaTradeBackend.Swagger;

public static class SwaggerMiddlewareExtensions
{
	public static IApplicationBuilder UseViaTradeSwagger(this IApplicationBuilder app)
	{
		var env = app.ApplicationServices.GetRequiredService<IWebHostEnvironment>();

		if (env.IsDevelopment())
		{
			app.UseSwagger();
			app.UseSwaggerUI(options =>
			{
				options.SwaggerEndpoint("/swagger/v1/swagger.yaml", "ViaTrade API v1 yaml");
				options.SwaggerEndpoint("/swagger/v1/swagger.json", "ViaTrade API v1 json");
				options.RoutePrefix = string.Empty;
				options.ConfigObject.AdditionalItems.Add("withCredentials", true);
			});
		}
		return app;
	}
}
