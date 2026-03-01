using System.Reflection;
using Application.Intarfaces;
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
using Microsoft.OpenApi;
using StackExchange.Redis;
using ViaTradeBackend.BackgroundServices;
using ViaTradeBackend.Handler;
using ViaTradeBackend.Middleware;
using ViaTradeBackend.OptionsSetup;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Custom auth service handler
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("ActiveSession", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.AddRequirements(new ActiveSessionRequirement());
    });

builder.Services.AddScoped<IAuthorizationHandler, ActiveSessionHandler>();

builder.Services.Configure<JwtOptions>(
    builder.Configuration.GetSection("Jwt")
);

builder.Services.Configure<AuthCookiOptions>(
    builder.Configuration.GetSection("AuthCookies")
);

builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var configuration = ConfigurationOptions.Parse(builder.Configuration.GetConnectionString("RedisLocalDevRiten") ?? throw new NullReferenceException());
    return ConnectionMultiplexer.Connect(configuration);
});

builder.Services.AddHostedService<SessionCleanupService>();

builder.Services.AddScoped<UserRedisRepository>();
builder.Services.AddScoped<ISessionRepository, SessionRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITokenCacheRepository, TokenCacheRepository>();
builder.Services.AddScoped<IJwtHelper, JwtHelper>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

var connectionString = builder.Configuration.GetConnectionString("MySqlLocalDevRiten") ?? throw new NullReferenceException();

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseMySql(
        connectionString,
        ServerVersion.AutoDetect(connectionString)
    );
});

builder.Services.AddSingleton<
    IConfigureOptions<JwtBearerOptions>,
    JwtBearerOptionsSetup>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer();

builder.Services.AddAuthorization();

builder.Services.AddControllers();

// === SWAGGER CONFIGURATION START ===
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "ViaTrade API",
        Version = "v1",
        Description = "API для торговой платформы ViaTrade"
    });

    // * Опционально: XML-комментарии
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        options.IncludeXmlComments(xmlPath);

});
// === SWAGGER CONFIGURATION END ===

var app = builder.Build();

// === SWAGGER MIDDLEWARE START ===
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "ViaTrade API v1");
        options.RoutePrefix = string.Empty;

        // Включаем отправку Cookie
        options.ConfigObject.AdditionalItems.Add("withCredentials", true);
    });
}
// === SWAGGER MIDDLEWARE END ===

// Configure the HTTP request pipeline.

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
