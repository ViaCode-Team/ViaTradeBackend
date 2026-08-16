using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using ViaTrade.Api.BackgroundServices;
using ViaTrade.Api.Cookies;
using ViaTrade.Api.Handler;
using ViaTrade.Api.OptionsSetup;
using ViaTrade.Application.Auth;
using ViaTrade.Application.Auth.Interfaces;
using ViaTrade.Application.Common.Interfaces;
using ViaTrade.Application.Common.Interfaces.Repositories;
using ViaTrade.Application.Instruments;
using ViaTrade.Application.Instruments.Interfaces;
using ViaTrade.Application.Notes;
using ViaTrade.Application.Notes.Interfaces;
using ViaTrade.Application.Notifications.Interfaces;
using ViaTrade.Application.Reminders;
using ViaTrade.Application.Reminders.Interfaces;
using ViaTrade.Application.Strategies;
using ViaTrade.Application.Strategies.Interfaces;
using ViaTrade.Application.Trades;
using ViaTrade.Application.Trades.Interfaces;
using ViaTrade.Application.Users;
using ViaTrade.Application.Users.Interfaces;
using ViaTrade.Configuration.Options;
using ViaTrade.Infrastructure.DataBase;
using ViaTrade.Infrastructure.DataBase.Repositories;
using ViaTrade.Infrastructure.Notifications;
using ViaTrade.Infrastructure.Redis.Repositories;
using ViaTrade.Infrastructure.Services;
using ViaTrade.Infrastructure.Utils;

namespace ViaTrade.Api;

public static class DependencyInjection
{
	public static IServiceCollection AddApplicationLayer(this IServiceCollection services)
	{
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

	public static IServiceCollection AddInfrastructureLayer(this IServiceCollection services)
	{
		services.AddDatabase();
		services.AddRedis();
		services.AddTelegramNotifications();
		services.AddRepositories();

		services.AddScoped<IUnitOfWork, EfUnitOfWork>();
		services.AddSingleton<ISeparateContextQueryExecutor, SeparateContextQueryExecutor>();

		services.AddHostedService<SessionCleanupService>();
		services.AddHostedService<TelegramReminderPublisherService>();
		services.AddHostedService<ReminderCleanupService>();

		return services;
	}

	public static IServiceCollection AddAuthLayer(this IServiceCollection services)
	{
		services.AddSingleton<IAuthCookieService, AuthCookieService>();

		services.AddJwtAuthentication();
		services.AddApplicationAuthorization();

		return services;
	}

	private static IServiceCollection AddTelegramNotifications(this IServiceCollection services)
	{
		services.AddSingleton<INotificationPublisher, RedisStreamNotificationPublisher>();

		return services;
	}

	private static IServiceCollection AddDatabase(this IServiceCollection services)
	{
		services.AddDbContext<AppDbContext>(
			(serviceProvider, options) =>
			{
				var connectionStrings = serviceProvider.GetRequiredService<IOptions<ConnectionStringsSettings>>().Value;
				var connectionString = connectionStrings.MySql;

				options
					.UseMySql(
						connectionString,
						ServerVersion.AutoDetect(connectionString),
						mySqlOptions => mySqlOptions.EnableStringComparisonTranslations()
					)
					.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
			}
		);

		return services;
	}

	private static IServiceCollection AddRedis(this IServiceCollection services)
	{
		services.AddSingleton<IConnectionMultiplexer>(serviceProvider =>
		{
			var connectionStrings = serviceProvider.GetRequiredService<IOptions<ConnectionStringsSettings>>().Value;
			var connectionString = connectionStrings.Redis;
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
