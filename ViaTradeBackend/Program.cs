using Application.Interfaces;
using Application.Interfaces.Repositories.Database;
using Application.Interfaces.Repositories.Redis;
using Application.Interfaces.Services;
using Application.Interfaces.Utils;
using Application.Mappings;
using Application.Services;
using Domain.Entities.Redis;
using Domain.Models.ConfigOptions;
using Infrastructure.Repositories.DataBase;
using Infrastructure.Repositories.Redis;
using Infrastructure.Services;
using Infrastructure.Utils;
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
builder.Services.AddScoped<IRedisRepository<TgTokenEntity>, TgTokenRepository>();
builder.Services.AddScoped<ISessionRepository, SessionRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<ITradeRepository, TradeRepository>();
builder.Services.AddScoped<ITradeTypeRepository, TradeTypeRepository>();
builder.Services.AddScoped<ITradeRemindRepository, TradeRemindRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserTradeStrategyRepository, UserTradeStrategyRepository>();
builder.Services.AddScoped<IUserStrategyTradeCodeRepository, UserStrategyTradeCodeRepository>();
builder.Services.AddScoped<ITradeStrategyRepository, TradeStrategyRepository>();
builder.Services.AddScoped<ITradeCodeRepository, TradeCodeRepository>();
builder.Services.AddScoped<INoteRepository, NoteRepository>();

// Application services
builder.Services.AddScoped<IStrategyService, StrategyService>();
builder.Services.AddScoped<ITradeResultsService, TradeResultsService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
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
