using System.Text.Json.Serialization;
using Infrastructure.Configuration;
using Microsoft.AspNetCore.Mvc;
using ViaTradeBackend;
using ViaTradeBackend.Middleware;
using ViaTradeBackend.Swagger;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplicationLayer();
builder.Services.AddInfrastructureLayer(builder.Configuration);
builder.Services.AddAuthLayer(builder.Configuration);
builder.Services.Configure<AnalyzerDataOption>(builder.Configuration.GetSection("AnalyzerData"));
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ExceptionHandlingMiddleware>();

builder
	.Services.AddControllers()
	.AddJsonOptions(options =>
	{
		options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
	});

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
	options.InvalidModelStateResponseFactory = actionContext =>
	{
		var problem = new ValidationProblemDetails(actionContext.ModelState)
		{
			Status = StatusCodes.Status400BadRequest,
			Title = "Validation Failed",
			Type = "https://httpstatuses.io/400",
			Detail = "One or more validation errors occurred.",
			Instance = actionContext.HttpContext.Request.Path,
		};

		problem.Extensions["code"] = "validation_failed";
		problem.Extensions["traceId"] = actionContext.HttpContext.TraceIdentifier;

		var result = new BadRequestObjectResult(problem);
		result.ContentTypes.Add("application/problem+json");
		return result;
	};
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddViaTradeSwagger();

var app = builder.Build();

app.UseViaTradeSwagger();
app.UseExceptionHandler();
app.UseHttpsRedirection();
app.UseMiddleware<ProblemDetailsStatusCodeMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
