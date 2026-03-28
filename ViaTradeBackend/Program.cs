using Application.Interfaces;
using Application.Interfaces.Auth;
using Application.Interfaces.Database;
using Application.Interfaces.Redis;
using Domain.Models;
using Infrastructure.Repositories.Redis;
using Infrastructure.Repositoryes.DataBase;
using Infrastructure.Repositoryes.Redis;
using Infrastructure.Services;
using Infrastructure.Utils;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using ViaTradeBackend.BackgroundServices;
using ViaTradeBackend.Handler;
using ViaTradeBackend.Middleware;
using ViaTradeBackend.OptionsSetup;
using ViaTradeBackend.Swagger;

var builder = WebApplication.CreateBuilder(args);

// AUTHORIZATION & POLICIES
// Custom policy for active session validation
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("ActiveSession", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.AddRequirements(new ActiveSessionRequirement());
    });

builder.Services.AddScoped<IAuthorizationHandler, ActiveSessionHandler>();

// OPTIONS CONFIGURATION
// Bind configuration sections to strongly-typed options
builder.Services.Configure<JwtOptions>(
    builder.Configuration.GetSection("Jwt")
);

builder.Services.Configure<AuthCookiOptions>(
    builder.Configuration.GetSection("AuthCookies")
);

builder.Services.AddOptions<AnalyzerDataOption>()
    .Configure<IConfiguration>((options, config) =>
    {
        var section = config.GetSection("AnalyzerData");
        var activeProfile = section["ActiveProfile"]
            ?? throw new InvalidOperationException("AnalyzerData:ActiveProfile is not set");

        var profileSection = section.GetSection($"Profiles:{activeProfile}");
        if (!profileSection.Exists())
        {
            throw new InvalidOperationException($"Profile '{activeProfile}' not found in AnalyzerData:Profiles");
        }

        // Bind selected profile to options instance
        profileSection.Bind(options);
    });

// REDIS SETUP
// Register Redis connection as singleton (shared across the app)
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var configuration = ConfigurationOptions.Parse(
        builder.Configuration.GetConnectionString("RedisLocalDevRiten")
        ?? throw new NullReferenceException("Redis connection string is missing"));

    return ConnectionMultiplexer.Connect(configuration);
});

// Background service for expired sessions cleanup
builder.Services.AddHostedService<SessionCleanupService>();

// SERVICES REGISTRATION
// Repositories
builder.Services.AddScoped<UserRedisRepository>();
builder.Services.AddScoped<ISessionRepository, SessionRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
// Application services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IJwtHelper, JwtHelper>();
builder.Services.AddScoped<IFileReader, TradeFileReader>();
//builder.Services.AddScoped<CsvHelper>();
builder.Services.AddScoped<ITradeDataBuilder, TradeDataBuilder>();
// DATABASE SETUP (MySQL)
var connectionString = builder.Configuration.GetConnectionString("MySqlLocalDevRiten")
    ?? throw new NullReferenceException("MySQL connection string is missing");

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseMySql(
        connectionString,
        ServerVersion.AutoDetect(connectionString)
    );
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
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Custom Swagger configuration (see Swagger/ folder)
builder.Services.AddViaTradeSwagger();

// BUILD & MIDDLEWARE PIPELINE
var app = builder.Build();

// Register AnalyzerDataOptions with profile selection logic


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
