using ViaTrade.Api;
using ViaTrade.Api.Middleware;
using ViaTrade.Api.Swagger;
using ViaTrade.Application;
using ViaTrade.Configuration;
using ViaTrade.Configuration.Options;
using ViaTrade.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder
	.Services.AddConfigurationLayer(builder.Configuration)
	.AddApplicationLayer()
	.AddInfrastructureLayer(builder.Configuration)
	.AddApiLayer();

builder.Services.AddBackgroundServices();

if (builder.Environment.IsDevelopment())
	builder.Services.AddViaTradeSwagger();

var emailSettings = builder.Configuration.GetSection("Email").Get<EmailSettings>() ?? new EmailSettings();
builder.Services.AddCors(options =>
	options.AddDefaultPolicy(policy =>
		policy.WithOrigins(emailSettings.AllowedOrigins).AllowAnyHeader().AllowAnyMethod().AllowCredentials()
	)
);

var app = builder.Build();

app.UseForwardedHeaders();

app.UseExceptionHandler();
app.UseMiddleware<ProblemDetailsStatusCodeMiddleware>();

if (app.Environment.IsDevelopment())
{
#if DEBUG
	app.UseDeveloperExceptionPage();
#endif

	app.UseSwaggerWithUi();

	app.ApplyDatabaseMigrations();
}

app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
