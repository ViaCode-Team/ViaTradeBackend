using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using ViaTrade.Application.Auth.Common;
using ViaTrade.Application.Auth.Login;
using ViaTrade.Application.Common.Validation;
using ViaTrade.Application.Signals.Common;

namespace ViaTrade.Application;

public static class DependencyInjection
{
	public static IServiceCollection AddApplicationLayer(this IServiceCollection services)
	{
		services.AddMediator(options =>
		{
			options.ServiceLifetime = ServiceLifetime.Scoped;
			options.Assemblies = [typeof(LoginCommand)];
			options.PipelineBehaviors = [typeof(ValidationBehavior<,>)];
		});

		services.AddValidatorsFromAssemblyContaining<LoginCommandValidator>();

		services.AddScoped<AuthTokenFactory>();
		services.AddScoped<SignalReader>();

		return services;
	}
}
