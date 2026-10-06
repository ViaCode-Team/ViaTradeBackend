using Ardalis.Specification;
using Ardalis.Specification.EntityFrameworkCore;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Scrutor;
using StackExchange.Redis;
using ViaTrade.Api.BackgroundServices;
using ViaTrade.Api.Cookies;
using ViaTrade.Api.Handler;
using ViaTrade.Api.OptionsSetup;
using ViaTrade.Application.Auth.Common;
using ViaTrade.Application.Auth.Common.Abstractions;
using ViaTrade.Application.Common.Abstractions;
using ViaTrade.Application.Common.Abstractions.Repositories;
using ViaTrade.Application.Common.Validation;
using ViaTrade.Application.Notes.Common.Abstractions;
using ViaTrade.Application.Notifications.Common.Abstractions;
using ViaTrade.Application.Reminders.Common.Abstractions;
using ViaTrade.Application.Signals.Common;
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
using ViaTrade.Infrastructure.Utils;

namespace ViaTrade.Api;

public static class DependencyInjection
{
	public static IServiceCollection AddApplicationLayer(this IServiceCollection services)
	{
		services.AddSingleton<IJwtHelper, JwtHelper>();
		services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();

		services.AddScoped<ITradeDataBuilder, TradeDataBuilder>();
		services.AddScoped<IFileReader, TradeFileReader>();
		services.AddScoped<AuthTokenFactory>();
		services.AddScoped<SignalReader>();

		services.Scan(scan =>
			scan.FromAssembliesOf(typeof(IQuery<>))
				.AddClasses(
					classes =>
						classes
							.Where(type => !type.IsGenericTypeDefinition)
							.AssignableToAny(
								typeof(IQueryHandler<,>),
								typeof(ICommandHandler<>),
								typeof(ICommandHandler<,>)
							),
					publicOnly: false
				)
				.UsingRegistrationStrategy(RegistrationStrategy.Throw)
				.AsImplementedInterfaces(IsHandlerInterface)
				.WithScopedLifetime()
		);

		services.Scan(scan =>
			scan.FromAssembliesOf(typeof(IQuery<>))
				.AddClasses(
					classes => classes.Where(type => !type.IsGenericTypeDefinition).AssignableTo(typeof(IValidator<>)),
					publicOnly: false
				)
				.AsImplementedInterfaces(type =>
					type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IValidator<>)
				)
				.WithScopedLifetime()
		);

		services.Decorate(typeof(ICommandHandler<>), typeof(ValidatingCommandHandler<>));
		services.Decorate(typeof(ICommandHandler<,>), typeof(ValidatingCommandHandler<,>));
		services.Decorate(typeof(IQueryHandler<,>), typeof(ValidatingQueryHandler<,>));

		return services;
	}

	public static IServiceCollection AddInfrastructureLayer(
		this IServiceCollection services,
		IConfiguration configuration
	)
	{
		services.AddDatabase(configuration);
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

	private static bool IsHandlerInterface(Type type)
	{
		if (!type.IsGenericType)
			return false;

		var definition = type.GetGenericTypeDefinition();
		return definition == typeof(IQueryHandler<,>)
			|| definition == typeof(ICommandHandler<>)
			|| definition == typeof(ICommandHandler<,>);
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
		services.AddScoped(typeof(IReadRepository<>), typeof(ReadEfRepository<>));
		services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
		services.AddSingleton<ISpecificationEvaluator>(SpecificationEvaluator.Default);

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

		services.AddSingleton<ISessionRepository, SessionRedisRepository>();

		services.AddScoped<ITradeRepository, TradeEfRepository>();

		services.AddScoped<IStrategyRepository, StrategyEfRepository>();

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

		services.AddSingleton<IAuthorizationHandler, ActiveSessionHandler>();

		return services;
	}
}
