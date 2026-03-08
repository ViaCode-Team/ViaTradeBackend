namespace ViaTradeBackend.Swagger
{
    public static class SwaggerMiddlewareExtensions
    {
        public static IApplicationBuilder UseViaTradeSwagger(this IApplicationBuilder app)
        {
            var env = app.ApplicationServices.GetRequiredService<IWebHostEnvironment>();

            if (env.IsDevelopment())
            {
                app.UseSwagger(); // Только JSON для SwaggerUI
                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint("/swagger/v1/swagger.json", "ViaTrade API v1");
                    options.RoutePrefix = string.Empty;
                    options.ConfigObject.AdditionalItems.Add("withCredentials", true);
                });
            }
            return app;
        }
    }
}