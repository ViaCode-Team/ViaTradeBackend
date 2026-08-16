using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ViaTrade.Configuration.Options;
using ViaTrade.Configuration.Validation;

namespace ViaTrade.Configuration;

public static class ApplicationSettingsServiceCollectionExtensions
{
	public static IServiceCollection AddApplicationSettings(
		this IServiceCollection services,
		IConfiguration configuration
	)
	{
		services.AddValidation();
		services.AddRootOptions(configuration);
		services.AddSectionOptions(configuration);

		return services;
	}

	private static void AddRootOptions(this IServiceCollection services, IConfiguration configuration)
	{
		services.AddOptions<ApplicationSettings>().Bind(configuration).ValidateOnStart();
	}

	private static void AddValidation(this IServiceCollection services)
	{
		services.AddSingleton<IValidateOptions<ApplicationSettings>, ApplicationSettingsDataAnnotationsValidator>();

		services.AddSingleton<IValidateOptions<ApplicationSettings>, ApplicationSettingsConsistencyValidator>();
	}

	private static void AddSectionOptions(this IServiceCollection services, IConfiguration configuration)
	{
		ArgumentNullException.ThrowIfNull(configuration);

		services
			.AddOptions<ConnectionStringsSettings>()
			.BindConfiguration(nameof(ApplicationSettings.ConnectionStrings));
		services.AddOptions<JwtSettings>().BindConfiguration(nameof(ApplicationSettings.Jwt));
		services.AddOptions<AuthCookieSettings>().BindConfiguration(nameof(ApplicationSettings.AuthCookies));
		services.AddOptions<TelegramBotSettings>().BindConfiguration(nameof(ApplicationSettings.TelegramBot));
		services
			.AddOptions<NotificationStreamSettings>()
			.BindConfiguration(nameof(ApplicationSettings.TelegramNotifications));
		services.AddOptions<ReminderCleanupSettings>().BindConfiguration(nameof(ApplicationSettings.ReminderCleanup));
		services.AddOptions<ReminderLimitsSettings>().BindConfiguration(nameof(ApplicationSettings.ReminderLimits));
		services.AddOptions<AnalyzerDataSettings>().BindConfiguration(nameof(ApplicationSettings.AnalyzerData));
		services.AddOptions<ServiceSecuritySettings>().BindConfiguration(nameof(ApplicationSettings.ServiceSecurity));
	}
}
