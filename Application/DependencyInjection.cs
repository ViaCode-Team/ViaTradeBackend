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
			// Any type from an assembly containing ICommand<T>, IQuery<T>, or their handlers.
			options.Assemblies = [typeof(LoginCommand)];
			// Any class implementing IPipelineBehavior<TMessage, TResponse>.
			options.PipelineBehaviors = [typeof(ValidationBehavior<,>)];
		});

		services.AddValidation();

		services.AddScoped<AuthTokenFactory>();
		services.AddScoped<SignalReader>();

		return services;
	}

	public static IServiceCollection AddValidation(this IServiceCollection services)
	{
		// Any type from the assembly containing IValidator<T> implementations.
		services.AddValidatorsFromAssemblyContaining<LoginCommandValidator>();

		return services;
	}
}
