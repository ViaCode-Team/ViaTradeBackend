using Application.Auth;
using Application.Auth.Interfaces;
using Application.Common.Interfaces;
using Application.Interfaces;
using Application.Notes;
using Application.Notes.Interfaces;
using Application.Reminders;
using Application.Reminders.Interfaces;
using Application.Strategies;
using Application.Strategies.Interfaces;
using Application.TradeCodes;
using Application.TradeCodes.Interfaces;
using Application.Trades;
using Application.Trades.Interfaces;
using Application.Trades.Services;
using Application.Users;
using Application.Users.Interfaces;
using Infrastructure.Configuration;
using Infrastructure.Repositories.DataBase;
using Infrastructure.Repositories.Redis;
using Infrastructure.Services;
using Infrastructure.Utils;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using ViaTradeBackend.BackgroundServices;
using ViaTradeBackend.Handler;
using ViaTradeBackend.OptionsSetup;

namespace ViaTradeBackend;

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
		services.AddScoped<ITradeResultsService, TradeResultsService>();
		services.AddScoped<ITradeDataBuilder, TradeDataBuilder>();
		services.AddScoped<IFileReader, TradeFileReader>();

		services.AddScoped<IStrategyCommandService, StrategyCommandService>();
		services.AddScoped<IStrategyQueryService, StrategyQueryService>();
		services.AddScoped<ITradeCodeQueryService, TradeCodeQueryService>();

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
		IConfiguration configuration)
	{
		services.AddDatabase(configuration);
		services.AddRedis(configuration);
		services.AddRepositories();

		services.AddScoped<IUnitOfWork, EfUnitOfWork>();

		services.AddHostedService<SessionCleanupService>();

		services.AddMapster();

		return services;
	}

	public static IServiceCollection AddAuthLayer(
		this IServiceCollection services,
		IConfiguration configuration)
	{
		services.Configure<JwtOptions>(configuration.GetSection("Jwt"));
		services.Configure<ServiceSecurity>(configuration.GetSection("ServiceSecurity"));
		services.Configure<AuthCookieOptions>(configuration.GetSection("AuthCookies"));

		services.AddJwtAuthentication();
		services.AddApplicationAuthorization();

		return services;
	}

	private static IServiceCollection AddDatabase(
		this IServiceCollection services,
		IConfiguration configuration)
	{
		var connectionString =
			configuration.GetConnectionString("MySql")
			?? throw new InvalidOperationException(
				"Connection string 'MySql' is not configured.");

		services.AddDbContext<AppDbContext>(options =>
		{
			options
				.UseMySql(
					connectionString,
					ServerVersion.AutoDetect(connectionString),
					mySqlOptions =>
						mySqlOptions.EnableStringComparisonTranslations())
				.UseQueryTrackingBehavior(
					QueryTrackingBehavior.NoTracking);
		});

		return services;
	}

	private static IServiceCollection AddRedis(
		this IServiceCollection services,
		IConfiguration configuration)
	{
		var connectionString =
			configuration.GetConnectionString("Redis")
			?? throw new InvalidOperationException(
				"Connection string 'Redis' is not configured.");

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

		services.AddScoped<ITgTokenRepository, TgTokenRedisRepository>();
		services.AddScoped<ISessionRepository, SessionRedisRepository>();
		services.AddScoped<IRefreshTokenRepository, RefreshTokenRedisRepository>();

		services.AddScoped<ITradeRepository, TradeEfRepository>();
		services.AddScoped<ITradeTypeRepository, TradeTypeEfRepository>();

		services.AddScoped<ITradeStrategyRepository, TradeStrategyEfRepository>();
		services.AddScoped<IUserTradeStrategyRepository, UserTradeStrategyEfRepository>();
		services.AddScoped<IUserStrategyTradeCodeRepository, UserStrategyTradeCodeEfRepository>();

		services.AddScoped<ITradeCodeRepository, TradeCodeEfRepository>();

		services.AddScoped<IReminderRepository, ReminderEfRepository>();
		services.AddScoped<INoteRepository, NoteEfRepository>();
		services.AddScoped<IUserRepository, UserEfRepository>();

		return services;
	}

	private static IServiceCollection AddJwtAuthentication(this IServiceCollection services)
	{
		services.AddSingleton<
			IConfigureOptions<JwtBearerOptions>,
			JwtBearerOptionsSetup>();

		services
			.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
			.AddJwtBearer();

		return services;
	}

	private static IServiceCollection AddApplicationAuthorization(this IServiceCollection services)
	{
		services
			.AddAuthorizationBuilder()
			.SetDefaultPolicy(
				new AuthorizationPolicyBuilder()
					.RequireAuthenticatedUser()
					.AddRequirements(new ActiveSessionRequirement())
					.Build());
		
		services.AddScoped<IAuthorizationHandler, ActiveSessionHandler>();

		return services;
	}
}
