using System.Text.Json;
using System.Text.Json.Serialization;
using Infrastructure.Configuration;
using ViaTradeBackend;
using ViaTradeBackend.Middleware;
using ViaTradeBackend.Swagger;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplicationLayer();

builder.Services.AddInfrastructureLayer(builder.Configuration);

builder.Services.AddAuthLayer(builder.Configuration);

builder.Services.Configure<AnalyzerDataOption>(builder.Configuration.GetSection("AnalyzerData"));

builder
	.Services.AddControllers()
	.AddJsonOptions(options =>
	{
		options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;

		options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
	});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddViaTradeSwagger();

var app = builder.Build();

app.UseViaTradeSwagger();

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();

app.UseMiddleware<ProblemDetailsStatusCodeMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
