using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Formatters;
using ViaTrade.Api.Middleware;
using ViaTrade.Api.Routing;
using ViaTrade.Api.Swagger;

namespace ViaTrade.Api;

public static class WebServiceCollectionExtensions
{
	public static IServiceCollection AddWebPresentation(this IServiceCollection services)
	{
		services.AddProblemDetails();
		services.AddExceptionHandler<ExceptionHandlingMiddleware>();
		services.Configure<ForwardedHeadersOptions>(options =>
		{
			options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
			options.KnownIPNetworks.Clear();
			options.KnownProxies.Clear();
			options.KnownProxies.Add(IPAddress.Loopback);
			options.KnownProxies.Add(IPAddress.IPv6Loopback);
		});

		services
			.AddControllers(options =>
			{
				options.Conventions.Add(new RouteTokenTransformerConvention(new CamelCaseRouteTokenTransformer()));

				var jsonInputFormatter = options.InputFormatters.OfType<SystemTextJsonInputFormatter>().Single();

				jsonInputFormatter.SupportedMediaTypes.Clear();
				jsonInputFormatter.SupportedMediaTypes.Add("application/json");
			})
			.AddJsonOptions(options =>
			{
				options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
				options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
				options.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
			});

		services.Configure<ApiBehaviorOptions>(options =>
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

		services.AddEndpointsApiExplorer();
		services.AddViaTradeSwagger();

		return services;
	}
}
