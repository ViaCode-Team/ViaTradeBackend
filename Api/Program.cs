using ViaTrade.Api;
using ViaTrade.Api.Middleware;
using ViaTrade.Api.Swagger;
using ViaTrade.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplicationSettings(builder.Configuration);
builder.Services.AddApplicationLayer();
builder.Services.AddInfrastructureLayer();
builder.Services.AddAuthLayer();
builder.Services.AddWebPresentation();

var app = builder.Build();

app.UseForwardedHeaders();
app.UseViaTradeSwagger();
app.UseExceptionHandler();
app.UseHttpsRedirection();
app.UseMiddleware<ProblemDetailsStatusCodeMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
