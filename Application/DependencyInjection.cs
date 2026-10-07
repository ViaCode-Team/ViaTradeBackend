using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Scrutor;
using ViaTrade.Application.Auth.Common;
using ViaTrade.Application.Common.Abstractions;
using ViaTrade.Application.Common.Validation;
using ViaTrade.Application.Signals.Common;

namespace ViaTrade.Application;

public static class DependencyInjection
{
	public static IServiceCollection AddApplicationLayer(this IServiceCollection services)
	{
		services.AddScoped<AuthTokenFactory>();
		services.AddScoped<SignalReader>();

		services.Scan(scan =>
			scan.FromAssembliesOf(typeof(IQuery<>))
				.AddClasses(
					classes =>
						classes
							.Where(type => !type.IsGenericTypeDefinition)
							.AssignableToAny(
								typeof(IQueryHandler<,>),
								typeof(ICommandHandler<>),
								typeof(ICommandHandler<,>)
							),
					publicOnly: false
				)
				.UsingRegistrationStrategy(RegistrationStrategy.Throw)
				.AsImplementedInterfaces(IsHandlerInterface)
				.WithScopedLifetime()
		);

		services.Scan(scan =>
			scan.FromAssembliesOf(typeof(IQuery<>))
				.AddClasses(
					classes => classes.Where(type => !type.IsGenericTypeDefinition).AssignableTo(typeof(IValidator<>)),
					publicOnly: false
				)
				.AsImplementedInterfaces(type =>
					type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IValidator<>)
				)
				.WithScopedLifetime()
		);

		services.Decorate(typeof(ICommandHandler<>), typeof(ValidatingCommandHandler<>));
		services.Decorate(typeof(ICommandHandler<,>), typeof(ValidatingCommandHandler<,>));
		services.Decorate(typeof(IQueryHandler<,>), typeof(ValidatingQueryHandler<,>));

		return services;
	}

	private static bool IsHandlerInterface(Type type)
	{
		if (!type.IsGenericType)
			return false;

		var definition = type.GetGenericTypeDefinition();
		return definition == typeof(IQueryHandler<,>)
			|| definition == typeof(ICommandHandler<>)
			|| definition == typeof(ICommandHandler<,>);
	}
}
