using System.Reflection;
using Mapster;
using MapsterMapper;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Utils;

public static class MappingConfig
{
	public static IServiceCollection AddMapster(this IServiceCollection services)
	{
		var config = new TypeAdapterConfig();

		config.Scan(Assembly.GetExecutingAssembly());

		services.AddSingleton(config);
		services.AddScoped<IMapper, ServiceMapper>();

		return services;
	}
}
