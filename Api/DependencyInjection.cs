using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ViaTrade.Api.BackgroundServices;
using ViaTrade.Api.Middleware;
using ViaTrade.Api.ModelBinding;
using ViaTrade.Api.Routing;
using ViaTrade.Api.Security.Authentication;
using ViaTrade.Api.Security.Authentication.Cookies;
using ViaTrade.Api.Security.Authorization;
using ViaTrade.Api.Security.UserContext;
using ViaTrade.Application.Common.Abstractions;
using ViaTrade.Infrastructure.DataBase;

namespace ViaTrade.Api;

public static class DependencyInjection
{
	public static IServiceCollection AddApiLayer(this IServiceCollection services)
	{
		services.AddAuth();
		services.AddApiMvc();
		services.AddApiErrorHandling();

		return services;
	}

	public static IServiceCollection AddBackgroundServices(this IServiceCollection services)
	{
		services.AddHostedService<SessionCleanupService>();
		services.AddHostedService<TelegramReminderPublisherService>();
		services.AddHostedService<ReminderCleanupService>();

		return services;
	}

	public static void ApplyDatabaseMigrations(this IApplicationBuilder app)
	{
		using IServiceScope scope = app.ApplicationServices.CreateScope();

		using AppDbContext dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

		dbContext.Database.Migrate();
	}

	public static IServiceCollection AddApiMvc(this IServiceCollection services)
	{
		services.AddTransient<IApplicationModelProvider, RequestPropertiesApplicationModelProvider>();

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
				options.Conventions.Add(new RequestPropertiesConvention());

				var jsonInputFormatter = options.InputFormatters.OfType<SystemTextJsonInputFormatter>().Single();
				jsonInputFormatter.SupportedMediaTypes.Clear();
				jsonInputFormatter.SupportedMediaTypes.Add("application/json");
				options.InputFormatters.Insert(0, new IgnorePropertiesJsonInputFormatter(jsonInputFormatter));

				var bodyModelBinderProvider = options.ModelBinderProviders.OfType<BodyModelBinderProvider>().Single();
				options.ModelBinderProviders.Insert(
					0,
					new RequestPropertiesModelBinderProvider(bodyModelBinderProvider)
				);
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
					Status = StatusCodes.Status422UnprocessableEntity,
					Title = "Validation Failed",
					Type = "https://httpstatuses.io/422",
					Detail = "One or more validation errors occurred.",
					Instance = actionContext.HttpContext.Request.Path,
				};

				problem.Extensions["code"] = "validation_failed";
				problem.Extensions["traceId"] = actionContext.HttpContext.TraceIdentifier;

				var result = new UnprocessableEntityObjectResult(problem);
				result.ContentTypes.Add("application/problem+json");
				return result;
			};
		});

		return services;
	}

	private static IServiceCollection AddAuth(this IServiceCollection services)
	{
		services.AddHttpContextAccessor();
		services.AddScoped<IUserContext, HttpUserContext>();
		services.AddScoped<IOptionalUserContext, HttpOptionalUserContext>();
		services.AddSingleton<IAuthCookieService, AuthCookieService>();

		services.AddJwtAuthentication();
		services.AddApplicationAuthorization();

		return services;
	}

	private static IServiceCollection AddApiErrorHandling(this IServiceCollection services)
	{
		services.AddProblemDetails();
		services.AddExceptionHandler<ExceptionHandlingMiddleware>();

		return services;
	}

	private static IServiceCollection AddJwtAuthentication(this IServiceCollection services)
	{
		services.AddSingleton<IConfigureOptions<JwtBearerOptions>, JwtBearerOptionsSetup>();

		services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();

		return services;
	}

	private static IServiceCollection AddApplicationAuthorization(this IServiceCollection services)
	{
		var defaultPolicy = new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme)
			.RequireAuthenticatedUser()
			.AddRequirements(new ActiveSessionRequirement())
			.Build();

		services.AddAuthorizationBuilder().SetDefaultPolicy(defaultPolicy).SetFallbackPolicy(defaultPolicy);

		services.AddSingleton<IAuthorizationHandler, ActiveSessionHandler>();

		return services;
	}
}
