using Application.Auth.Interfaces;
using Application.Common.Behaviors;
using Application.Common.Interfaces;
using Application.Interfaces;
using Application.Mappings;
using Application.Notes.Interfaces;
using Application.Reminds.Interfaces;
using Application.Strategies.Interfaces;
using Application.TradeCodes.Interfaces;
using Application.Trades.Interfaces;
using Application.Trades.Services;
using Application.Users.Interfaces;
using Infrastructure.Configuration;
using Infrastructure.Repositories.DataBase;
using Infrastructure.Repositories.Redis;
using Infrastructure.Services;
using Infrastructure.Utils;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System.Text.Json;
using System.Text.Json.Serialization;
using ViaTradeBackend.BackgroundServices;
using ViaTradeBackend.Handler;
using ViaTradeBackend.Middleware;
using ViaTradeBackend.OptionsSetup;
using ViaTradeBackend.Swagger;

var builder = WebApplication.CreateBuilder(args);

// AUTHORIZATION & POLICIES
// Custom policy for active session validation (Set as default)
builder.Services.AddAuthorizationBuilder()
	.SetDefaultPolicy(new AuthorizationPolicyBuilder()
		.RequireAuthenticatedUser()
		.AddRequirements(new ActiveSessionRequirement())
		.Build());

builder.Services.AddScoped<IAuthorizationHandler, ActiveSessionHandler>();

// OPTIONS CONFIGURATION
// Bind configuration sections to strongly-typed options
builder.Services.Configure<JwtOptions>(
	builder.Configuration.GetSection("Jwt")
);

builder.Services.Configure<ServiceSecurity>(
	builder.Configuration.GetSection("ServiceSecurity")
);

builder.Services.Configure<AuthCookieOptions>(
	builder.Configuration.GetSection("AuthCookies")
);

builder.Services.Configure<AnalyzerDataOption>(
	builder.Configuration.GetSection("AnalyzerData")
);

// REDIS SETUP
// Register Redis connection as singleton (shared across the app)
builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
{
	var configuration = ConfigurationOptions.Parse(
		builder.Configuration.GetConnectionString("Redis")
		?? throw new NullReferenceException("Redis connection string is missing"));

	return ConnectionMultiplexer.Connect(configuration);
});

// Background service for expired sessions cleanup
builder.Services.AddHostedService<SessionCleanupService>();

// SERVICES REGISTRATION
builder.Services.AddMapster();

// Repositories
builder.Services.AddScoped<UserRedisRepository>();
builder.Services.AddScoped<ITgTokenRepository, TgTokenRedisRepository>();
builder.Services.AddScoped<ISessionRepository, SessionRedisRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRedisRepository>();
builder.Services.AddScoped<ITradeRepository, TradeEfRepository>();
builder.Services.AddScoped<ITradeTypeRepository, TradeTypeEfRepository>();
builder.Services.AddScoped<ITradeRemindRepository, TradeRemindEfRepository>();
builder.Services.AddScoped<IUserRepository, UserEfRepository>();
builder.Services.AddScoped<IUserTradeStrategyRepository, UserTradeStrategyEfRepository>();
builder.Services.AddScoped<IUserStrategyTradeCodeRepository, UserStrategyTradeCodeEfRepository>();
builder.Services.AddScoped<ITradeStrategyRepository, TradeStrategyEfRepository>();
builder.Services.AddScoped<ITradeCodeRepository, TradeCodeEfRepository>();
builder.Services.AddScoped<INoteRepository, NoteEfRepository>();

// MediatR
builder.Services.AddMediatR(cfg =>
{
	cfg.RegisterServicesFromAssembly(typeof(TradeResultsService).Assembly);
	cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
	cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
	cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(UnitOfWorkBehavior<,>));
});

// Domain Events
builder.Services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();

// Unit of Work
builder.Services.AddScoped<IUnitOfWork, EfUnitOfWork>();

// Application services
builder.Services.AddScoped<ITradeResultsService, TradeResultsService>();
builder.Services.AddScoped<IJwtHelper, JwtHelper>();
builder.Services.AddScoped<IPasswordHasher, BCryptPasswordHasher>();
builder.Services.AddScoped<IFileReader, TradeFileReader>();
builder.Services.AddScoped<ITradeDataBuilder, TradeDataBuilder>();

// DATABASE SETUP (MySQL)
var connectionString = builder.Configuration.GetConnectionString("MySql")
	?? throw new NullReferenceException("MySQL connection string is missing");

builder.Services.AddDbContext<AppDbContext>(options =>
{
	options.UseMySql(
		connectionString,
		ServerVersion.AutoDetect(connectionString),
		mySqlOptions => mySqlOptions.EnableStringComparisonTranslations()
	).UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
});

// AUTHENTICATION (JWT)
// Configure JwtBearer options via dedicated setup class
builder.Services.AddSingleton<IConfigureOptions<JwtBearerOptions>, JwtBearerOptionsSetup>();

builder.Services.AddAuthentication(options =>
{
	options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
	options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer();

// Enable authorization services
builder.Services.AddAuthorization();

// API CONTROLLERS & SWAGGER
builder.Services
	.AddControllers()
	.AddJsonOptions(options =>
	{
		options.JsonSerializerOptions.DefaultIgnoreCondition =
			JsonIgnoreCondition.WhenWritingNull;

		options.JsonSerializerOptions.Converters.Add(
			new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
	});

builder.Services.AddEndpointsApiExplorer();

// Custom Swagger configuration (see Swagger/ folder)
builder.Services.AddViaTradeSwagger();

// BUILD & MIDDLEWARE PIPELINE
var app = builder.Build();

// Swagger UI (Development only, configured in SwaggerMiddlewareExtensions)
app.UseViaTradeSwagger();

// Global exception handling middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();

// HTTPS redirection
app.UseHttpsRedirection();

app.UseMiddleware<ProblemDetailsStatusCodeMiddleware>();

// Authentication & Authorization middleware (order matters!)
app.UseAuthentication();
app.UseAuthorization();

// Map controller endpoints
app.MapControllers();

// APPLICATION STARTUP
app.Run();
