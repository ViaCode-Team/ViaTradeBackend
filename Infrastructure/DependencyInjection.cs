using Ardalis.Specification;
using Ardalis.Specification.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using ViaTrade.Application.Auth.Common.Abstractions;
using ViaTrade.Application.Common.Abstractions;
using ViaTrade.Application.Common.Abstractions.Repositories;
using ViaTrade.Application.Notes.Common.Abstractions;
using ViaTrade.Application.Notifications.Common.Abstractions;
using ViaTrade.Application.Reminders.Common.Abstractions;
using ViaTrade.Application.Strategies.Common.Abstractions;
using ViaTrade.Application.Trades.Common.Abstractions;
using ViaTrade.Application.Users.Common;
using ViaTrade.Application.Users.Common.Abstractions;
using ViaTrade.Configuration;
using ViaTrade.Configuration.Options;
using ViaTrade.Infrastructure.DataBase;
using ViaTrade.Infrastructure.DataBase.Interceptors;
using ViaTrade.Infrastructure.DataBase.Repositories;
using ViaTrade.Infrastructure.DataBase.Repositories.Generic;
using ViaTrade.Infrastructure.Notifications;
using ViaTrade.Infrastructure.Redis.Entities;
using ViaTrade.Infrastructure.Redis.Keys;
using ViaTrade.Infrastructure.Redis.Repositories;
using ViaTrade.Infrastructure.Services;

namespace ViaTrade.Infrastructure;

public static class DependencyInjection
{
	public static IServiceCollection AddInfrastructureLayer(
		this IServiceCollection services,
		IConfiguration configuration
	)
	{
		services.AddDatabase(configuration);
		services.AddDatabaseRepositories();

		services.AddRedis();
		services.AddCacheRepositories();

		services.AddTelegramNotifications();

		services.AddServices();

		return services;
	}

	private static IServiceCollection AddTelegramNotifications(this IServiceCollection services)
	{
		services.AddSingleton<INotificationPublisher, RedisStreamNotificationPublisher>();

		return services;
	}

	private static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
	{
		var connectionSettings = configuration.GetConnectionStrings();
		var dbSettings = configuration.GetDatabaseSettings();

		services.AddSingleton<MySqlExceptionTranslationInterceptor>();

		services.AddDbContextPool<AppDbContext>(
			(serviceProvider, options) =>
			{
				var interceptor = serviceProvider.GetRequiredService<MySqlExceptionTranslationInterceptor>();
				options
					.UseMySql(
						connectionSettings.MySql,
						ServerVersion.AutoDetect(connectionSettings.MySql),
						mySqlOptions =>
						{
							mySqlOptions.EnableStringComparisonTranslations();
							mySqlOptions.EnableRetryOnFailure(
								maxRetryCount: dbSettings.MaxRetryCount,
								maxRetryDelay: TimeSpan.FromSeconds(dbSettings.MaxRetryDelaySeconds),
								errorNumbersToAdd: null
							);
						}
					)
					.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
					.AddInterceptors(interceptor);
			}
		);

		services.AddScoped<IUnitOfWork, EfUnitOfWork>();
		services.AddSingleton<ISeparateContextQueryExecutor, SeparateContextQueryExecutor>();

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

	private static IServiceCollection AddCacheRepositories(this IServiceCollection services)
	{
		services.AddSingleton<ICacheRepository<UserRedisEntity>>(
			serviceProvider => new BaseRedisRepository<UserRedisEntity>(
				serviceProvider.GetRequiredService<IConnectionMultiplexer>().GetDatabase(),
				RedisKeys.Cache.Users
			)
		);
		services.AddSingleton<ICacheRepository<TelegramTokenEntity>>(
			serviceProvider => new BaseRedisRepository<TelegramTokenEntity>(
				serviceProvider.GetRequiredService<IConnectionMultiplexer>().GetDatabase(),
				RedisKeys.Cache.TelegramTokens
			)
		);

		return services;
	}

	private static IServiceCollection AddDatabaseRepositories(this IServiceCollection services)
	{
		services.AddScoped(typeof(IReadRepository<>), typeof(ReadEfRepository<>));
		services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
		services.AddSingleton<ISpecificationEvaluator>(SpecificationEvaluator.Default);

		services.AddSingleton<ISessionRepository, SessionRedisRepository>();
		services.AddScoped<ITradeRepository, TradeEfRepository>();
		services.AddScoped<IStrategyRepository, StrategyEfRepository>();
		services.AddScoped<IReminderRepository, ReminderEfRepository>();
		services.AddScoped<INoteRepository, NoteEfRepository>();
		services.AddScoped<IUserRepository, UserEfRepository>();

		return services;
	}

	private static IServiceCollection AddServices(this IServiceCollection services)
	{
		services.AddSingleton<IJwtHelper, JwtHelper>();

		services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();

		services.AddScoped<ITradeDataBuilder, TradeDataBuilder>();

		services.AddScoped<IFileReader, TradeFileReader>();

		return services;
	}
}
