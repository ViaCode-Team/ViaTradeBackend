using System.Reflection;
using System.Text.Json;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Scrutor;
using ViaTrade.Api;
using ViaTrade.Api.Attributes.Binding;
using ViaTrade.Application.Auth.Common;
using ViaTrade.Application.Auth.Common.Abstractions;
using ViaTrade.Application.Common.Abstractions;
using ViaTrade.Application.Common.Abstractions.Repositories;
using ViaTrade.Application.Common.Models;
using ViaTrade.Application.Common.Validation;
using ViaTrade.Application.Notes.Common;
using ViaTrade.Application.Notes.Common.Abstractions;
using ViaTrade.Application.Notes.Get;
using ViaTrade.Application.Reminders.Common.Abstractions;
using ViaTrade.Application.Strategies.Common.Abstractions;
using ViaTrade.Application.Trades.Common.Abstractions;
using ViaTrade.Application.Users.Common;
using ViaTrade.Application.Users.Common.Abstractions;
using ViaTrade.Configuration.Options;
using ViaTrade.Infrastructure.DataBase;
using ViaTrade.Infrastructure.DataBase.Repositories;
using ViaTrade.Infrastructure.DataBase.Repositories.Generic;
using Xunit;

namespace ViaTrade.Tests;

public sealed class CqrsRegistrationTests
{
	[Fact]
	public void DuplicateHandlerContractsAreRejectedDuringRegistration()
	{
		var services = new ServiceCollection();
		services.AddScoped<IQueryHandler<GetNoteQuery, NoteResult>, GetNoteHandler>();

		Assert.Throws<DuplicateTypeRegistrationException>(() => services.AddApplicationLayer());
	}

	[Fact]
	public void EveryHandlerIsRegisteredOnceAndResolvesWithinItsScope()
	{
		var services = CreateServices();
		var handlers = typeof(IQuery<>)
			.Assembly.GetTypes()
			.Where(type => type is { IsClass: true, IsAbstract: false, IsGenericTypeDefinition: false })
			.SelectMany(type =>
				type.GetInterfaces().Where(IsHandler).Select(contract => (Type: type, Contract: contract))
			)
			.ToList();
		Assert.NotEmpty(handlers);
		using var provider = services.BuildServiceProvider(
			new ServiceProviderOptions { ValidateOnBuild = true, ValidateScopes = true }
		);
		using var first = provider.CreateScope();
		using var second = provider.CreateScope();

		foreach (var handler in handlers)
		{
			var registration = Assert.Single(
				services,
				descriptor => !descriptor.IsKeyedService && descriptor.ServiceType == handler.Contract
			);
			Assert.Equal(ServiceLifetime.Scoped, registration.Lifetime);
			var instance = first.ServiceProvider.GetRequiredService(handler.Contract);
			var definition = handler.Contract.GetGenericTypeDefinition();
			var decorator = definition switch
			{
				var type when type == typeof(ICommandHandler<>) => typeof(ValidatingCommandHandler<>),
				var type when type == typeof(ICommandHandler<,>) => typeof(ValidatingCommandHandler<,>),
				_ => typeof(ValidatingQueryHandler<,>),
			};
			Assert.IsType(decorator.MakeGenericType(handler.Contract.GetGenericArguments()), instance);
			var inner = instance
				.GetType()
				.GetField("_inner", BindingFlags.Instance | BindingFlags.NonPublic)!
				.GetValue(instance);
			Assert.IsType(handler.Type, inner);
			Assert.Same(instance, first.ServiceProvider.GetRequiredService(handler.Contract));
			Assert.NotSame(instance, second.ServiceProvider.GetRequiredService(handler.Contract));
		}
	}

	[Fact]
	public void EveryValidatorIsRegisteredOnceAndResolvesWithinItsScope()
	{
		var services = CreateServices();
		var validators = typeof(IQuery<>)
			.Assembly.GetTypes()
			.Where(type => type is { IsClass: true, IsAbstract: false, IsGenericTypeDefinition: false })
			.SelectMany(type =>
				type.GetInterfaces()
					.Where(contract =>
						contract.IsGenericType && contract.GetGenericTypeDefinition() == typeof(IValidator<>)
					)
					.Select(contract => (Type: type, Contract: contract))
			)
			.ToList();
		Assert.NotEmpty(validators);
		using var provider = services.BuildServiceProvider();
		using var first = provider.CreateScope();
		using var second = provider.CreateScope();

		foreach (var validator in validators)
		{
			var registration = Assert.Single(services, descriptor => descriptor.ServiceType == validator.Contract);
			Assert.Equal(ServiceLifetime.Scoped, registration.Lifetime);
			var instance = first.ServiceProvider.GetRequiredService(validator.Contract);
			Assert.IsType(validator.Type, instance);
			Assert.Same(instance, first.ServiceProvider.GetRequiredService(validator.Contract));
			Assert.NotSame(instance, second.ServiceProvider.GetRequiredService(validator.Contract));
		}
	}

	[Fact]
	public void EveryApiActionReceivesItsRegisteredHandlerFromServices()
	{
		var services = CreateServices();
		var actions = typeof(DependencyInjection)
			.Assembly.GetTypes()
			.Where(type => typeof(ControllerBase).IsAssignableFrom(type))
			.SelectMany(type =>
				type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
			)
			.Where(method => method.GetCustomAttributes<HttpMethodAttribute>().Any())
			.ToList();
		Assert.NotEmpty(actions);

		foreach (var action in actions)
		{
			var parameter = Assert.Single(action.GetParameters(), parameter => IsHandler(parameter.ParameterType));
			Assert.NotNull(parameter.GetCustomAttribute<FromServicesAttribute>());
			Assert.Single(
				services,
				descriptor => !descriptor.IsKeyedService && descriptor.ServiceType == parameter.ParameterType
			);
		}
	}

	[Fact]
	public void ApiRequestParametersHaveAtLeastOneClientBoundProperty()
	{
		var parameters = typeof(DependencyInjection)
			.Assembly.GetTypes()
			.Where(type => typeof(ControllerBase).IsAssignableFrom(type))
			.SelectMany(type =>
				type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
			)
			.Where(method => method.GetCustomAttributes<HttpMethodAttribute>().Any())
			.SelectMany(method => method.GetParameters())
			.Where(parameter =>
				parameter
					.ParameterType.GetInterfaces()
					.Any(contract =>
						contract == typeof(ICommand)
						|| contract.IsGenericType
							&& (
								contract.GetGenericTypeDefinition() == typeof(ICommand<>)
								|| contract.GetGenericTypeDefinition() == typeof(IQuery<>)
							)
					)
			);

		foreach (var parameter in parameters)
		{
			var ignored = parameter.GetCustomAttribute<IgnorePropertiesAttribute>()?.PropertyNames ?? [];
			Assert.True(
				parameter.ParameterType.GetProperties().Any(property => !ignored.Contains(property.Name)),
				$"{parameter.Member.DeclaringType!.Name}.{parameter.Member.Name} must construct {parameter.ParameterType.Name} inside the action."
			);
		}
	}

	[Fact]
	public void RequestsAndResultsAreRecordsAndDoNotExposeEntities()
	{
		var requests = typeof(IQuery<>)
			.Assembly.GetTypes()
			.Where(type => type is { IsClass: true, IsAbstract: false, IsGenericTypeDefinition: false })
			.Select(type => (Type: type, Markers: type.GetInterfaces().Where(IsRequest).ToList()))
			.Where(item => item.Markers.Count > 0)
			.ToList();
		Assert.NotEmpty(requests);

		foreach (var request in requests)
		{
			Assert.NotNull(request.Type.GetMethod("<Clone>$"));
			foreach (var marker in request.Markers.Where(type => type.IsGenericType))
				AssertResultContract(marker.GetGenericArguments()[0], []);
		}
	}

	[Fact]
	public void PageResultKeepsTheExistingJsonProperties()
	{
		var result = new PageResult<int>([1, 2], 3, 1, 2);
		var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
		using var json = JsonDocument.Parse(JsonSerializer.Serialize(result, options));
		var names = json.RootElement.EnumerateObject().Select(property => property.Name).Order().ToList();
		Assert.Equal(new[] { "items", "page", "pageSize", "totalCount", "totalPages" }, names);
		Assert.Equal(1, json.RootElement.GetProperty("page").GetInt32());
		Assert.Equal(2, json.RootElement.GetProperty("totalPages").GetInt32());
	}

	[Fact]
	public void SessionDataStillReadsExistingRedisJson()
	{
		const string legacyJson = """
			{"Id":"session","UserId":7,"UserAgent":"browser","CreatedAt":"2026-01-01T00:00:00Z","LastSeen":"2026-01-02T00:00:00Z","ExpiresAt":"2026-01-03T00:00:00Z"}
			""";
		var session = JsonSerializer.Deserialize<SessionData>(legacyJson);
		Assert.NotNull(session);
		Assert.Equal("session", session.Id);
		Assert.Equal(7, session.UserId);
		Assert.Equal(DateTime.Parse("2026-01-03T00:00:00Z").ToUniversalTime(), session.ExpiresAt);
	}

	private static ServiceCollection CreateServices()
	{
		var services = new ServiceCollection();
		services.AddOptions();
		services.Configure<AnalyzerDataSettings>(options => options.SourcePath = AppContext.BaseDirectory);
		services.AddApplicationLayer();
		services.AddDbContext<AppDbContext>(options => options.UseSqlite("Data Source=:memory:"));
		services.AddScoped(typeof(IReadRepository<>), typeof(ReadEfRepository<>));
		services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
		services.AddScoped<IUnitOfWork, EfUnitOfWork>();
		services.AddScoped<ITradeRepository, TradeEfRepository>();
		services.AddScoped<IStrategyRepository, StrategyEfRepository>();
		services.AddScoped<INoteRepository, NoteEfRepository>();
		services.AddScoped<IReminderRepository, ReminderEfRepository>();
		services.AddScoped<IUserRepository, UserEfRepository>();
		services.AddSingleton(DispatchProxy.Create<ISessionRepository, UnusedRepositoryStub>());
		services.AddSingleton(DispatchProxy.Create<ICacheRepository<TelegramTokenEntity>, UnusedRepositoryStub>());
		return services;
	}

	private static bool IsHandler(Type type)
	{
		if (!type.IsGenericType)
			return false;

		var definition = type.GetGenericTypeDefinition();
		return definition == typeof(IQueryHandler<,>)
			|| definition == typeof(ICommandHandler<>)
			|| definition == typeof(ICommandHandler<,>);
	}

	private static bool IsRequest(Type type)
	{
		if (type == typeof(ICommand))
			return true;

		if (!type.IsGenericType)
			return false;

		var definition = type.GetGenericTypeDefinition();
		return definition == typeof(IQuery<>) || definition == typeof(ICommand<>);
	}

	private static void AssertResultContract(Type type, HashSet<Type> visited)
	{
		if (!visited.Add(type))
			return;

		Assert.False(type.Namespace?.StartsWith("ViaTrade.Domain.Entities", StringComparison.Ordinal) == true);
		if (type.IsGenericType)
		{
			foreach (var argument in type.GetGenericArguments())
				AssertResultContract(argument, visited);
		}

		if (type.Assembly != typeof(IQuery<>).Assembly)
			return;

		Assert.EndsWith("Result", type.Name.Split('`')[0]);
		Assert.NotNull(type.GetMethod("<Clone>$"));
		foreach (var property in type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
			AssertResultContract(property.PropertyType, visited);
	}

	public class UnusedRepositoryStub : DispatchProxy
	{
		protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
		{
			throw new InvalidOperationException($"Unexpected repository call: {targetMethod?.Name}");
		}
	}
}
