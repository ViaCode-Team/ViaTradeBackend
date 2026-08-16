using Application.Auth;
using Application.Auth.Interfaces;
using Application.Auth.Models;
using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Application.Instruments;
using Application.Instruments.Interfaces;
using Application.Notes;
using Application.Notes.Interfaces;
using Application.Notifications.Interfaces;
using Application.Notifications.Models;
using Application.Reminders;
using Application.Reminders.Interfaces;
using Application.Reminders.Models;
using Application.Strategies;
using Application.Strategies.Interfaces;
using Application.Trades;
using Application.Trades.Interfaces;
using Application.Users;
using Application.Users.Interfaces;
using Application.Users.Models;
using Infrastructure.Configuration;
using Infrastructure.DataBase;
using Infrastructure.DataBase.Repositories;
using Infrastructure.Notifications;
using Infrastructure.Redis.Repositories;
using Infrastructure.Services;
using Infrastructure.Utils;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using ViaTradeBackend.BackgroundServices;
using ViaTradeBackend.Cookies;
using ViaTradeBackend.Handler;
using ViaTradeBackend.OptionsSetup;

namespace ViaTradeBackend;

public static class DependencyInjection
{
	public static IServiceCollection AddApplicationLayer(this IServiceCollection services, IConfiguration configuration)
	{
		var telegramBotOptions = configuration.GetSection("TelegramBot").Get<TelegramBotOptions>();
		if (telegramBotOptions == null || string.IsNullOrWhiteSpace(telegramBotOptions.BotUsername))
			throw new InvalidOperationException("Telegram bot options are not configured.");

		services.AddSingleton(telegramBotOptions);

		var authCookieOptions = configuration.GetSection("AuthCookies").Get<AuthCookieOptions>();
		if (authCookieOptions == null)
			throw new InvalidOperationException("Auth cookie options are not configured.");

		if (authCookieOptions.RefreshTokenExpiryDays < 1)
			throw new InvalidOperationException("Refresh token expiry must be at least one day.");

		if (authCookieOptions.AbsoluteSessionLifetimeDays < 1)
			throw new InvalidOperationException("Absolute session lifetime must be at least one day.");

		var jwtOptions = configuration.GetSection("Jwt").Get<JwtOptions>();
		if (jwtOptions == null || jwtOptions.AccessTokenMinutes < 1)
			throw new InvalidOperationException("Access token lifetime must be at least one minute.");

		services.AddSingleton(
			new SessionLifetimeOptions
			{
				AccessTokenLifetime = TimeSpan.FromMinutes(jwtOptions.AccessTokenMinutes),
				IdleLifetime = TimeSpan.FromDays(authCookieOptions.RefreshTokenExpiryDays),
				AbsoluteLifetime = TimeSpan.FromDays(authCookieOptions.AbsoluteSessionLifetimeDays),
			}
		);

		services.AddScoped<IJwtHelper, JwtHelper>();
		services.AddScoped<IPasswordHasher, BCryptPasswordHasher>();

		services.AddScoped<IAuthCommandService, AuthCommandService>();
		services.AddScoped<IAuthQueryService, AuthQueryService>();

		services.AddScoped<ITradeCommandService, TradeCommandService>();
		services.AddScoped<ITradeQueryService, TradeQueryService>();
		services.AddScoped<ISignalQueryService, SignalQueryService>();
		services.AddScoped<ITradeDataBuilder, TradeDataBuilder>();
		services.AddScoped<IFileReader, TradeFileReader>();

		services.AddScoped<IStrategyCommandService, StrategyCommandService>();
		services.AddScoped<IStrategyQueryService, StrategyQueryService>();
		services.AddScoped<IInstrumentQueryService, InstrumentQueryService>();

		services.AddScoped<INoteCommandService, NoteCommandService>();
		services.AddScoped<INoteQueryService, NoteQueryService>();

		services.AddScoped<IReminderCommandService, ReminderCommandService>();
		services.AddScoped<IReminderQueryService, ReminderQueryService>();

		services.AddScoped<IUserCommandService, UserCommandService>();
		services.AddScoped<IUserQueryService, UserQueryService>();

		return services;
	}

	public static IServiceCollection AddInfrastructureLayer(
		this IServiceCollection services,
		IConfiguration configuration
	)
	{
		services.AddDatabase(configuration);
		services.AddRedis(configuration);
		services.AddTelegramNotifications(configuration);
		services.AddReminderOptions(configuration);
		services.AddRepositories();

		services.AddScoped<IUnitOfWork, EfUnitOfWork>();
		services.AddSingleton<ISeparateContextQueryExecutor, SeparateContextQueryExecutor>();

		services.AddHostedService<SessionCleanupService>();
		services.AddHostedService<TelegramReminderPublisherService>();
		services.AddHostedService<ReminderCleanupService>();

		return services;
	}

	public static IServiceCollection AddAuthLayer(this IServiceCollection services, IConfiguration configuration)
	{
		services.Configure<JwtOptions>(configuration.GetSection("Jwt"));
		services.Configure<ServiceSecurity>(configuration.GetSection("ServiceSecurity"));
		services.Configure<AuthCookieOptions>(configuration.GetSection("AuthCookies"));
		services.AddSingleton<IAuthCookieService, AuthCookieService>();

		services.AddJwtAuthentication();
		services.AddApplicationAuthorization();

		return services;
	}

	private static IServiceCollection AddTelegramNotifications(
		this IServiceCollection services,
		IConfiguration configuration
	)
	{
		var streamOptions = configuration.GetSection("TelegramNotifications").Get<NotificationStreamOptions>();
		if (streamOptions == null || string.IsNullOrWhiteSpace(streamOptions.StreamName))
			throw new InvalidOperationException("Telegram notification stream options are not configured.");

		if (streamOptions.RedisDatabase < 0)
			throw new InvalidOperationException("Telegram notification Redis database cannot be negative.");

		if (streamOptions.MaxLength < 1 || streamOptions.ReminderPublishIntervalSeconds < 1)
			throw new InvalidOperationException("Telegram notification settings must be positive.");

		services.AddSingleton(streamOptions);

		services.AddSingleton<INotificationPublisher, RedisStreamNotificationPublisher>();

		return services;
	}

	private static IServiceCollection AddReminderOptions(this IServiceCollection services, IConfiguration configuration)
	{
		var reminderCleanupOptions = configuration.GetSection("ReminderCleanup").Get<ReminderCleanupOptions>();
		if (
			reminderCleanupOptions == null
			|| reminderCleanupOptions.RetentionDays < 1
			|| reminderCleanupOptions.CleanupIntervalHours < 1
		)
			throw new InvalidOperationException("Reminder cleanup settings must be positive.");

		var reminderLimitsOptions = configuration.GetSection("ReminderLimits").Get<ReminderLimitsOptions>();
		if (reminderLimitsOptions == null || reminderLimitsOptions.MaxRemindersPerUser < 1)
			throw new InvalidOperationException("Reminder limit must be positive.");

		services.AddSingleton(reminderCleanupOptions);
		services.AddSingleton(reminderLimitsOptions);

		return services;
	}

	private static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
	{
		var connectionString =
			configuration.GetConnectionString("MySql")
			?? throw new InvalidOperationException("Connection string 'MySql' is not configured.");

		void ConfigureDbContext(DbContextOptionsBuilder options)
		{
			options
				.UseMySql(
					connectionString,
					ServerVersion.AutoDetect(connectionString),
					mySqlOptions => mySqlOptions.EnableStringComparisonTranslations()
				)
				.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
		}

		services.AddDbContext<AppDbContext>(ConfigureDbContext);

		return services;
	}

	private static IServiceCollection AddRedis(this IServiceCollection services, IConfiguration configuration)
	{
		var connectionString =
			configuration.GetConnectionString("Redis")
			?? throw new InvalidOperationException("Connection string 'Redis' is not configured.");

		services.AddSingleton<IConnectionMultiplexer>(_ =>
		{
			var options = ConfigurationOptions.Parse(connectionString);

			return ConnectionMultiplexer.Connect(options);
		});
		return services;
	}

	private static IServiceCollection AddRepositories(this IServiceCollection services)
	{
		services.AddScoped<UserRedisRepository>();

		services.AddScoped<ITelegramTokenRepository, TelegramTokenRedisRepository>();
		services.AddScoped<ISessionRepository, SessionRedisRepository>();

		services.AddScoped<ITradeRepository, TradeEfRepository>();
		services.AddScoped<ITradeTypeRepository, TradeTypeEfRepository>();

		services.AddScoped<IStrategyRepository, StrategyEfRepository>();
		services.AddScoped<IUserStrategyRepository, UserStrategyEfRepository>();
		services.AddScoped<IUserStrategyInstrumentRepository, UserStrategyInstrumentEfRepository>();

		services.AddScoped<IInstrumentRepository, InstrumentEfRepository>();

		services.AddScoped<IReminderRepository, ReminderEfRepository>();
		services.AddScoped<INoteRepository, NoteEfRepository>();
		services.AddScoped<IUserRepository, UserEfRepository>();

		return services;
	}

	private static IServiceCollection AddJwtAuthentication(this IServiceCollection services)
	{
		services.AddSingleton<IConfigureOptions<JwtBearerOptions>, JwtBearerOptionsSetup>();

		services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();

		return services;
	}

	private static IServiceCollection AddApplicationAuthorization(this IServiceCollection services)
	{
		var defaultPolicy = new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme)
			.RequireAuthenticatedUser()
			.AddRequirements(new ActiveSessionRequirement())
			.Build();

		services.AddAuthorizationBuilder().SetDefaultPolicy(defaultPolicy).SetFallbackPolicy(defaultPolicy);

		services.AddScoped<IAuthorizationHandler, ActiveSessionHandler>();

		return services;
	}
}
