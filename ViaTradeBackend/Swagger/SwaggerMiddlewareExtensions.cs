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
				options.SwaggerEndpoint("/swagger/v1/swagger.yaml", "ViaTrade Public API v1");
				options.SwaggerEndpoint("/swagger/internal/swagger.yaml", "ViaTrade Internal API v1");

				options.RoutePrefix = string.Empty;
				options.ConfigObject.AdditionalItems.Add("withCredentials", true);

				options.DisplayRequestDuration();
				options.DisplayOperationId();
			});
		}
		return app;
	}
}
