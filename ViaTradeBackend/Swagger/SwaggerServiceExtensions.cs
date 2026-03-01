using System.Reflection;
using Microsoft.OpenApi;

namespace ViaTradeBackend.Swagger
{
    public static class SwaggerServiceExtensions
    {
        public static IServiceCollection AddViaTradeSwagger(this IServiceCollection services)
        {
            services.AddEndpointsApiExplorer();

            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "ViaTrade API",
                    Version = "v1",
                    Description = "API для платформы инвестиционного анализа ViaTrade"
                });

                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                if (File.Exists(xmlPath))
                    options.IncludeXmlComments(xmlPath);
            });

            return services;
        }
    }
}