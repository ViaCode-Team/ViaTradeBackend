using Microsoft.Extensions.Configuration;
using ViaTrade.Configuration.Options;

namespace ViaTrade.Configuration;

public static class ConfigurationExtensions
{
	public static ConnectionStringsSettings GetConnectionStrings(this IConfiguration configuration)
	{
		var connectionSettings =
			configuration.GetSection(nameof(ApplicationSettings.ConnectionStrings)).Get<ConnectionStringsSettings>()
			?? throw new InvalidOperationException("ConnectionStrings section is missing.");

		if (string.IsNullOrEmpty(connectionSettings.MySql))
			throw new InvalidOperationException("MySQL connection string not found.");

		return connectionSettings;
	}

	public static DatabaseSettings GetDatabaseSettings(this IConfiguration configuration)
	{
		return configuration.GetSection(nameof(ApplicationSettings.Database)).Get<DatabaseSettings>()
			?? new DatabaseSettings();
	}
}
