using ViaTrade.Api;
using ViaTrade.Api.Middleware;
using ViaTrade.Api.Swagger;
using ViaTrade.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplicationSettings(builder.Configuration);
builder.Services.AddApplicationLayer();
builder.Services.AddInfrastructureLayer(builder.Configuration);
builder.Services.AddAuthLayer();
builder.Services.AddWebPresentation();

// ToDo: Uncomment and adjust the URL if the frontend is hosted on a different domain
//builder.Services.AddCors(options =>
//{
//	options.AddDefaultPolicy(p =>
//		p.WithOrigins("https://via_trade_backend").AllowAnyHeader().AllowAnyMethod().AllowCredentials()
//	);
//});

var app = builder.Build();

app.UseForwardedHeaders();
app.UseViaTradeSwagger(app.Environment);
app.UseExceptionHandler();
app.UseMiddleware<ProblemDetailsStatusCodeMiddleware>();

// ToDo: Uncomment if CORS is configured above
//app.UseCors();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
