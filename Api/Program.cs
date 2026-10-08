using ViaTrade.Api;
using ViaTrade.Api.Middleware;
using ViaTrade.Api.Swagger;
using ViaTrade.Application;
using ViaTrade.Configuration;
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

// ToDo: Uncomment and adjust the URL if the frontend is hosted on a different domain
//builder.Services.AddCors(options =>
//{
//	options.AddDefaultPolicy(p =>
//		p.WithOrigins("https://via_trade_backend").AllowAnyHeader().AllowAnyMethod().AllowCredentials()
//	);
//});

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

// ToDo: Uncomment if CORS is configured above
//app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
