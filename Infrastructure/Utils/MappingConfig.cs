using Mapster;
using MapsterMapper;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Application.Mappings;

public static class MappingConfig
{
	public static void ConfigureMapster(this IServiceCollection services)
	{
		var config = TypeAdapterConfig.GlobalSettings;

		// Scan the assembly for any IRegister implementations
		config.Scan(Assembly.GetExecutingAssembly());

		// We can add specific global configurations here if needed
		// For example, if we wanted to globally ignore HashPassword:
		//config.Default.Ignore("HashPassword");

		services.AddSingleton(config);
		services.AddScoped<IMapper, ServiceMapper>();
	}
}
