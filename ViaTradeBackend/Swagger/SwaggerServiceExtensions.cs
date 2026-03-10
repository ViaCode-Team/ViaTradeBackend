using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;

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

                // Включает учет C# Nullable Reference Types
                // string? -> nullable: true, string -> без флага
                options.SupportNonNullableReferenceTypes();

                options.CustomOperationIds(apiDesc =>
                {
                    return apiDesc.TryGetMethodInfo(out var methodInfo)
                        ? methodInfo.Name
                        : null;
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