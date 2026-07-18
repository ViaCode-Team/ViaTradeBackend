using Mapster;
using MapsterMapper;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Application.Mappings;

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
