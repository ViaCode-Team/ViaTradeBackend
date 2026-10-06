using System.Net;
using System.Net.Http.Json;
using System.Reflection;
using System.Text;
using System.Text.Json;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ViaTrade.Api;
using ViaTrade.Api.Controllers;
using ViaTrade.Api.Cookies;
using ViaTrade.Application.Auth.Common;
using ViaTrade.Application.Auth.Common.Abstractions;
using ViaTrade.Application.Auth.GetSessionsPage;
using ViaTrade.Application.Auth.Login;
using ViaTrade.Application.Auth.Register;
using ViaTrade.Application.Common.Abstractions;
using ViaTrade.Application.Common.Models;
using ViaTrade.Application.Common.Validation;
using ViaTrade.Application.Notes.GetStatistics;
using ViaTrade.Application.Notes.UpsertInstrument;
using ViaTrade.Application.Notes.UpsertStrategy;
using ViaTrade.Application.Reminders.Common;
using ViaTrade.Application.Reminders.Create;
using ViaTrade.Application.Reminders.ListDue;
using ViaTrade.Application.Reminders.Update;
using ViaTrade.Application.Signals.Common;
using ViaTrade.Application.Signals.GetHistoryPage;
using ViaTrade.Application.Signals.GetLatestPage;
using ViaTrade.Application.Strategies.GetInstrumentsPage;
using ViaTrade.Application.Strategies.SetSubscription;
using ViaTrade.Application.Trades.Common;
using ViaTrade.Application.Trades.Create;
using ViaTrade.Application.Trades.GetPage;
using ViaTrade.Application.Trades.GetProfitChart;
using ViaTrade.Application.Trades.Update;
using ViaTrade.Domain.Enums;
using Xunit;
using AppValidationException = ViaTrade.Application.Common.Exceptions.ValidationException;

namespace ViaTrade.Tests;

public sealed class ApplicationContractTests
{
	private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

	[Fact]
	public void ApiUsesApplicationContractsAndDoesNotContainRequestOrResponseModels()
	{
		var apiTypes = typeof(TradesController).Assembly.GetTypes();
		Assert.DoesNotContain(apiTypes, type => type.Namespace?.StartsWith("ViaTrade.Api.Contracts") == true);
		Assert.DoesNotContain(apiTypes, type => type.Name.EndsWith("Request") || type.Name.EndsWith("Response"));
		Assert.Equal(
			typeof(CreateTradeCommand),
			typeof(TradesController).GetMethod("CreateTrade")!.GetParameters()[0].ParameterType
		);
	}

	[Fact]
	public async Task NoteStatisticsActionConstructsItsQueryFromClaims()
	{
		var jwt = DispatchProxy.Create<IJwtHelper, JwtStub>();
		var controller = new NotesController(jwt)
		{
			ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() },
		};
		controller.Request.QueryString = new QueryString("?userId=invalid");
		var handler = new NoteStatisticsStub();
		using var cancellation = new CancellationTokenSource();

		var result = await controller.GetNoteStatistics(handler, cancellation.Token);

		Assert.Equal(new GetNoteStatisticsQuery(7), handler.LastQuery);
		Assert.Equal(cancellation.Token, handler.LastCancellationToken);
		Assert.Equal(new NoteStatisticsResult(3, 2, 1), result.Value);
	}

	[Fact]
	public async Task CommandsWithResultsRejectInvalidInputsBeforeExecutingTheHandler()
	{
		var inner = new CreateTradeStub();
		var handler = new ValidatingCommandHandler<CreateTradeCommand, TradeResult>(
			inner,
			[new CreateTradeCommandValidator()]
		);
		var input = ValidTrade() with { Quantity = 0, InstrumentId = 0 };

		var exception = await Assert.ThrowsAsync<AppValidationException>(() =>
			handler.HandleAsync(ToCreateCommand(input))
		);

		Assert.Contains("quantity", exception.Errors.Keys);
		Assert.Contains("instrumentId", exception.Errors.Keys);
		Assert.Equal(0, inner.Calls);
	}

	[Theory]
	[InlineData(0)]
	[InlineData(-1)]
	[InlineData(double.NaN)]
	[InlineData(double.PositiveInfinity)]
	public void TradeValidationRejectsInvalidPrices(double price)
	{
		AssertTradeValidation(ValidTrade() with { OpenPrice = price }, false);
		AssertTradeValidation(ValidTrade() with { ClosePrice = price, ClosedAt = DateTime.UtcNow }, false);
	}

	[Fact]
	public void TradeValidationPreservesDatePriceAndEnumRules()
	{
		var command = ValidTrade();
		AssertTradeValidation(command, true);
		AssertTradeValidation(command with { ClosedAt = command.OpenedAt, ClosePrice = 10 }, true);
		AssertTradeValidation(command with { ClosedAt = command.OpenedAt.AddSeconds(-1), ClosePrice = 10 }, false);
		AssertTradeValidation(command with { ClosedAt = command.OpenedAt }, false);
		AssertTradeValidation(command with { ClosePrice = 10 }, false);
		AssertTradeValidation(command with { Signal = (TradeSignal)int.MaxValue }, false);
		AssertTradeValidation(command with { InstrumentId = 0 }, false);
		AssertTradeValidation(command with { TradeTypeId = 0 }, false);
		AssertTradeValidation(command with { Quantity = 0 }, false);
	}

	private static void AssertTradeValidation(UpdateTradeCommand command, bool valid)
	{
		Assert.Equal(valid, new UpdateTradeCommandValidator().Validate(command).IsValid);
		Assert.Equal(valid, new CreateTradeCommandValidator().Validate(ToCreateCommand(command)).IsValid);
	}

	[Theory]
	[InlineData(null)]
	[InlineData("")]
	public void CommandValidatorsRejectMissingText(string? text)
	{
		Assert.False(new LoginCommandValidator().Validate(new LoginCommand(text!, "password", "browser")).IsValid);
		Assert.False(
			new RegisterCommandValidator().Validate(new RegisterCommand(text!, "password", "browser")).IsValid
		);
		Assert.False(new LoginCommandValidator().Validate(new LoginCommand("user", text!, "browser")).IsValid);
		Assert.False(new RegisterCommandValidator().Validate(new RegisterCommand("user", text!, "browser")).IsValid);
		AssertTextValidation(text!, false);
	}

	[Fact]
	public void CommandValidatorsPreserveLengthLimits()
	{
		var login = new LoginCommand(new string('a', 64), new string('b', 72), "browser");
		AssertAuthValidation(login, true);
		AssertAuthValidation(login with { Password = "short" }, false);
		AssertAuthValidation(login with { Login = new string('a', 65) }, false);
		AssertAuthValidation(login with { Password = new string('a', 73) }, false);
		AssertTextValidation(new string('a', 1024), true);
		AssertTextValidation(new string('a', 1025), false);
	}

	private static void AssertAuthValidation(LoginCommand command, bool valid)
	{
		Assert.Equal(valid, new LoginCommandValidator().Validate(command).IsValid);
		Assert.Equal(
			valid,
			new RegisterCommandValidator()
				.Validate(new RegisterCommand(command.Login, command.Password, command.UserAgent))
				.IsValid
		);
	}

	private static void AssertTextValidation(string text, bool valid)
	{
		Assert.Equal(
			valid,
			new UpsertInstrumentNoteCommandValidator().Validate(new UpsertInstrumentNoteCommand(7, 1, text)).IsValid
		);
		Assert.Equal(
			valid,
			new UpsertStrategyNoteCommandValidator().Validate(new UpsertStrategyNoteCommand(7, 1, text)).IsValid
		);
		Assert.Equal(
			valid,
			new CreateReminderCommandValidator()
				.Validate(new CreateReminderCommand(7, 1, text, DateTime.UtcNow))
				.IsValid
		);
		Assert.Equal(
			valid,
			new UpdateReminderCommandValidator()
				.Validate(new UpdateReminderCommand(7, 1, text, DateTime.UtcNow))
				.IsValid
		);
	}

	[Fact]
	public async Task QueriesRejectInvalidPaginationBeforeExecutingTheHandler()
	{
		var inner = new TradesPageStub();
		var handler = new ValidatingQueryHandler<GetTradesPageQuery, PageResult<TradeResult>>(
			inner,
			[new GetTradesPageQueryValidator()]
		);
		var query = new GetTradesPageQuery(
			7,
			new TradeFilter(null, null, null, null, null),
			new TradeSearch(),
			new PageOptions { PageSize = 101 }
		);

		var exception = await Assert.ThrowsAsync<AppValidationException>(() => handler.HandleAsync(query));

		Assert.Contains("pageOptions.pageSize", exception.Errors.Keys);
		Assert.Equal(0, inner.Calls);
	}

	[Fact]
	public async Task AsyncRulesAndCancellationAreHonoredByTheDecorator()
	{
		var inner = new UpdateTradeStub();
		var validator = new InlineValidator<UpdateTradeCommand>();
		var ruleRan = false;
		validator
			.RuleFor(command => command.TradeId)
			.MustAsync(
				async (_, ct) =>
				{
					await Task.Yield();
					ct.ThrowIfCancellationRequested();
					ruleRan = true;
					return false;
				}
			);
		var handler = new ValidatingCommandHandler<UpdateTradeCommand>(inner, [validator]);
		var command = ValidTrade();

		await Assert.ThrowsAsync<AppValidationException>(() => handler.HandleAsync(command));
		Assert.True(ruleRan);
		Assert.Equal(0, inner.Calls);

		using var cancellation = new CancellationTokenSource();
		cancellation.Cancel();
		await Assert.ThrowsAnyAsync<OperationCanceledException>(() => handler.HandleAsync(command, cancellation.Token));
		Assert.Equal(0, inner.Calls);
	}

	[Fact]
	public void ResultJsonKeepsPublicFieldsAndExcludesDeliveryInternals()
	{
		AssertJsonProperties(
			new ReminderResult(1, "note", DateTime.UtcNow, null, null),
			"id",
			"text",
			"remindAt",
			"instrument",
			"deliveredAt"
		);
		AssertJsonProperties(
			new DueReminderResult(1, "note", DateTime.UtcNow, null, 7, "secret-recipient"),
			"id",
			"text",
			"remindAt",
			"instrument",
			"userId"
		);
		AssertJsonProperties(
			new SessionResult("session", 7, "browser", DateTime.UtcNow, DateTime.UtcNow, true),
			"id",
			"userId",
			"userAgent",
			"createdAt",
			"lastSeen",
			"isCurrent"
		);
	}

	[Fact]
	public void InstrumentFilterValidationKeepsLimitsAndPositiveIds()
	{
		var validator = new StrategyInstrumentFilterValidator();
		Assert.True(validator.Validate(new StrategyInstrumentFilter(null)).IsValid);
		Assert.True(validator.Validate(new StrategyInstrumentFilter(Enumerable.Range(1, 100).ToList())).IsValid);
		Assert.False(validator.Validate(new StrategyInstrumentFilter(Enumerable.Range(1, 101).ToList())).IsValid);
		Assert.False(validator.Validate(new StrategyInstrumentFilter([0])).IsValid);
	}

	[Fact]
	public void QueryValidatorsValidateSearchAndSortingWithoutMvc()
	{
		var trades = new GetTradesPageQuery(
			7,
			new TradeFilter(null, null, null, null, null),
			new TradeSearch(),
			new PageOptions()
		);
		Assert.False(
			new GetTradesPageQueryValidator()
				.Validate(trades with { TradeSearch = new TradeSearch { SearchText = new string('x', 101) } })
				.IsValid
		);
		Assert.True(new GetTradesPageQueryValidator().Validate(trades).IsValid);

		var signals = new GetLatestSignalsPageQuery(7, new LatestSignalFilter(), new SignalSort(), new PageOptions());
		Assert.True(new GetLatestSignalsPageQueryValidator().Validate(signals).IsValid);
		Assert.False(
			new GetLatestSignalsPageQueryValidator()
				.Validate(
					signals with
					{
						SignalSort = new SignalSort
						{
							SortBy = [SignalSortField.SignalDateAsc, SignalSortField.SignalDateDesc],
						},
					}
				)
				.IsValid
		);
		Assert.False(
			new GetLatestSignalsPageQueryValidator()
				.Validate(signals with { SignalSort = new SignalSort { SortBy = [(SignalSortField)int.MaxValue] } })
				.IsValid
		);
	}

	[Fact]
	public void SharedDateRangeValidationPreservesOptionalEndpointsAndEquality()
	{
		var validator = new ProfitChartFilterValidator();
		var date = new DateOnly(2026, 1, 1);
		Assert.True(validator.Validate(new ProfitChartFilter()).IsValid);
		Assert.True(validator.Validate(new ProfitChartFilter { StartDate = date }).IsValid);
		Assert.True(validator.Validate(new ProfitChartFilter { EndDate = date }).IsValid);
		Assert.True(validator.Validate(new ProfitChartFilter { StartDate = date, EndDate = date }).IsValid);
		Assert.False(validator.Validate(new ProfitChartFilter { StartDate = date.AddDays(1), EndDate = date }).IsValid);
		var history = new SignalHistoryFilter
		{
			StrategyId = 1,
			InstrumentId = 1,
			StartDate = date.ToDateTime(TimeOnly.MinValue),
			EndDate = date.AddDays(-1).ToDateTime(TimeOnly.MinValue),
		};
		Assert.False(new SignalHistoryFilterValidator().Validate(history).IsValid);
	}

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void SubscriptionValidationAllowsBothBooleanValues(bool isSubscribed)
	{
		var validator = new SetStrategySubscriptionCommandValidator();
		Assert.True(validator.Validate(new SetStrategySubscriptionCommand(7, 1, isSubscribed)).IsValid);
		Assert.False(validator.Validate(new SetStrategySubscriptionCommand(7, 1, null)).IsValid);
		Assert.False(validator.Validate(new SetStrategySubscriptionCommand(0, 1, isSubscribed)).IsValid);
		Assert.False(validator.Validate(new SetStrategySubscriptionCommand(7, 0, isSubscribed)).IsValid);
	}

	[Fact]
	public async Task HttpBindingCombinesCommandBodyRouteAndTrustedUserAndReturnsValidationProblems()
	{
		var builder = WebApplication.CreateBuilder();
		builder.WebHost.UseUrls("http://127.0.0.1:0");
		builder.Logging.ClearProviders();
		builder.Services.AddWebPresentation();
		builder.Services.AddControllers().AddApplicationPart(typeof(TradesController).Assembly);
		builder.Services.AddSingleton(DispatchProxy.Create<IJwtHelper, JwtStub>());
		var inner = new UpdateTradeStub();
		builder.Services.AddSingleton<ICommandHandler<UpdateTradeCommand>>(inner);
		builder.Services.AddSingleton<IValidator<UpdateTradeCommand>, UpdateTradeCommandValidator>();
		builder.Services.Decorate(typeof(ICommandHandler<>), typeof(ValidatingCommandHandler<>));
		var queryInner = new TradesPageStub();
		builder.Services.AddSingleton<IQueryHandler<GetTradesPageQuery, PageResult<TradeResult>>>(queryInner);
		builder.Services.AddSingleton<IValidator<GetTradesPageQuery>, GetTradesPageQueryValidator>();
		builder.Services.Decorate(typeof(IQueryHandler<,>), typeof(ValidatingQueryHandler<,>));
		await using var app = builder.Build();
		app.UseExceptionHandler();
		app.MapControllers();
		await app.StartAsync();
		var address = Assert.Single(
			app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses
		);
		using var client = new HttpClient { BaseAddress = new Uri(address) };

		using var valid = await client.PutAsJsonAsync(
			"/api/v1/trades/42",
			ValidTrade() with
			{
				UserId = 999,
				TradeId = 999,
			}
		);
		Assert.Equal(HttpStatusCode.NoContent, valid.StatusCode);
		Assert.Equal(7, inner.LastCommand!.UserId);
		Assert.Equal(42, inner.LastCommand.TradeId);
		Assert.Equal(10, inner.LastCommand.OpenPrice);

		using var invalid = await client.PutAsJsonAsync("/api/v1/trades/42", ValidTrade() with { Quantity = 0 });
		Assert.Equal(HttpStatusCode.BadRequest, invalid.StatusCode);
		using var problem = JsonDocument.Parse(await invalid.Content.ReadAsStringAsync());
		Assert.Equal("validation_failed", problem.RootElement.GetProperty("code").GetString());
		Assert.True(problem.RootElement.GetProperty("errors").TryGetProperty("quantity", out _));
		Assert.Equal(1, inner.Calls);

		using var malformed = await client.PutAsync(
			"/api/v1/trades/42",
			new StringContent("{", Encoding.UTF8, "application/json")
		);
		Assert.Equal(HttpStatusCode.BadRequest, malformed.StatusCode);
		using var invalidRoute = await client.PutAsJsonAsync("/api/v1/trades/0", ValidTrade());
		Assert.Equal(HttpStatusCode.BadRequest, invalidRoute.StatusCode);
		Assert.Equal(1, inner.Calls);

		using var invalidPage = await client.GetAsync("/api/v1/trades?pageSize=101");
		Assert.Equal(HttpStatusCode.BadRequest, invalidPage.StatusCode);
		using var pageProblem = JsonDocument.Parse(await invalidPage.Content.ReadAsStringAsync());
		Assert.True(
			pageProblem.RootElement.GetProperty("errors").TryGetProperty("pageOptions.pageSize", out _),
			pageProblem.RootElement.ToString()
		);
		Assert.Equal(0, queryInner.Calls);

		using var malformedPage = await client.GetAsync("/api/v1/trades?pageSize=invalid");
		Assert.Equal(HttpStatusCode.BadRequest, malformedPage.StatusCode);
		Assert.Equal(0, queryInner.Calls);

		using var validPage = await client.GetAsync(
			"/api/v1/trades?page=2&pageSize=3&searchText=test&signal=BUY&userId=invalid"
		);
		Assert.Equal(HttpStatusCode.OK, validPage.StatusCode);
		Assert.Equal(7, queryInner.LastQuery!.UserId);
		Assert.Equal(2, queryInner.LastQuery.PageOptions.Page);
		Assert.Equal(3, queryInner.LastQuery.PageOptions.PageSize);
		Assert.Equal("test", queryInner.LastQuery.TradeSearch.SearchText);
		Assert.Equal(TradeSignal.BUY, queryInner.LastQuery.TradeFilter.Signal);
		await app.StopAsync();
	}

	[Theory]
	[InlineData("/api/v1/sessions")]
	[InlineData("/api/v1/users")]
	public async Task AuthActionsReadCredentialsFromCommandAndUserAgentFromHeader(string path)
	{
		var builder = WebApplication.CreateBuilder();
		builder.WebHost.UseUrls("http://127.0.0.1:0");
		builder.Logging.ClearProviders();
		builder.Services.AddWebPresentation();
		builder.Services.AddControllers().AddApplicationPart(typeof(TradesController).Assembly);
		builder.Services.AddSingleton(DispatchProxy.Create<IJwtHelper, JwtStub>());
		var cookies = new AuthCookieStub();
		builder.Services.AddSingleton<IAuthCookieService>(cookies);
		var login = new AuthHandlerStub<LoginCommand>();
		var register = new AuthHandlerStub<RegisterCommand>();
		builder.Services.AddSingleton<ICommandHandler<LoginCommand, AuthTokensResult>>(login);
		builder.Services.AddSingleton<ICommandHandler<RegisterCommand, AuthTokensResult>>(register);
		builder.Services.AddSingleton<IValidator<LoginCommand>, LoginCommandValidator>();
		builder.Services.AddSingleton<IValidator<RegisterCommand>, RegisterCommandValidator>();
		builder.Services.Decorate(typeof(ICommandHandler<,>), typeof(ValidatingCommandHandler<,>));
		await using var app = builder.Build();
		app.UseExceptionHandler();
		app.MapControllers();
		await app.StartAsync();
		var address = Assert.Single(
			app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses
		);
		using var client = new HttpClient { BaseAddress = new Uri(address) };
		client.DefaultRequestHeaders.UserAgent.ParseAdd("trusted-browser/1.0");

		using var valid = await client.PostAsJsonAsync(
			path,
			new
			{
				login = "user",
				password = "password",
				userAgent = new { spoofed = true },
			}
		);
		Assert.Equal(HttpStatusCode.NoContent, valid.StatusCode);
		var received = (object?)login.LastCommand ?? register.LastCommand;
		var (receivedLogin, password, userAgent) = received switch
		{
			LoginCommand command => (command.Login, command.Password, command.UserAgent),
			RegisterCommand command => (command.Login, command.Password, command.UserAgent),
			_ => throw new InvalidOperationException("The auth handler did not receive a command."),
		};
		Assert.Equal("user", receivedLogin);
		Assert.Equal("password", password);
		Assert.Equal("trusted-browser/1.0", userAgent);
		Assert.Equal(1, cookies.Calls);

		using var invalid = await client.PostAsJsonAsync(path, new { login = "user", password = "short" });
		Assert.Equal(HttpStatusCode.BadRequest, invalid.StatusCode);
		using var problem = JsonDocument.Parse(await invalid.Content.ReadAsStringAsync());
		Assert.True(problem.RootElement.GetProperty("errors").TryGetProperty("password", out _));
		Assert.Equal(1, login.Calls + register.Calls);
		Assert.Equal(1, cookies.Calls);
	}

	[Theory]
	[InlineData("{}", false)]
	[InlineData("{\"isSubscribed\":null}", false)]
	[InlineData("{\"isSubscribed\":false,\"userId\":{},\"strategyId\":{}}", true)]
	[InlineData("{\"isSubscribed\":true,\"userId\":{},\"strategyId\":{}}", true)]
	public async Task SubscriptionBodyRequiresAnExplicitBoolean(string json, bool valid)
	{
		var builder = WebApplication.CreateBuilder();
		builder.WebHost.UseUrls("http://127.0.0.1:0");
		builder.Logging.ClearProviders();
		builder.Services.AddWebPresentation();
		builder.Services.AddControllers().AddApplicationPart(typeof(TradesController).Assembly);
		builder.Services.AddSingleton(DispatchProxy.Create<IJwtHelper, JwtStub>());
		var received = new List<object>();
		builder.Services.AddSingleton<ICommandHandler<SetStrategySubscriptionCommand>>(
			new CommandStub<SetStrategySubscriptionCommand>(received)
		);
		builder.Services.AddSingleton<
			IValidator<SetStrategySubscriptionCommand>,
			SetStrategySubscriptionCommandValidator
		>();
		builder.Services.Decorate(typeof(ICommandHandler<>), typeof(ValidatingCommandHandler<>));
		await using var app = builder.Build();
		app.UseExceptionHandler();
		app.MapControllers();
		await app.StartAsync();
		var address = Assert.Single(
			app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses
		);
		using var client = new HttpClient { BaseAddress = new Uri(address) };
		using var body = new StringContent(json, Encoding.UTF8, "application/json");
		using var response = await client.PatchAsync("/api/v1/strategies/42", body);
		if (valid)
		{
			Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
			var command = Assert.IsType<SetStrategySubscriptionCommand>(Assert.Single(received));
			Assert.Equal(7, command.UserId);
			Assert.Equal(42, command.StrategyId);
			using var document = JsonDocument.Parse(json);
			Assert.Equal(document.RootElement.GetProperty("isSubscribed").GetBoolean(), command.IsSubscribed);
		}
		else
		{
			Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
			using var problem = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
			Assert.True(problem.RootElement.GetProperty("errors").TryGetProperty("isSubscribed", out _));
			Assert.Empty(received);
		}

		await app.StopAsync();
	}

	[Theory]
	[InlineData("/api/v1/instruments/42/note", "PUT")]
	[InlineData("/api/v1/strategies/42/note", "PUT")]
	[InlineData("/api/v1/instruments/42/reminders", "POST")]
	[InlineData("/api/v1/reminders/42", "PUT")]
	public async Task TextCommandBodiesAreFlatAndUseTrustedIds(string path, string method)
	{
		var builder = WebApplication.CreateBuilder();
		builder.WebHost.UseUrls("http://127.0.0.1:0");
		builder.Logging.ClearProviders();
		builder.Services.AddWebPresentation();
		builder.Services.AddControllers().AddApplicationPart(typeof(TradesController).Assembly);
		builder.Services.AddSingleton(DispatchProxy.Create<IJwtHelper, JwtStub>());
		var received = new List<object>();
		builder.Services.AddSingleton<ICommandHandler<UpsertInstrumentNoteCommand>>(
			new CommandStub<UpsertInstrumentNoteCommand>(received)
		);
		builder.Services.AddSingleton<ICommandHandler<UpsertStrategyNoteCommand>>(
			new CommandStub<UpsertStrategyNoteCommand>(received)
		);
		builder.Services.AddSingleton<ICommandHandler<UpdateReminderCommand>>(
			new CommandStub<UpdateReminderCommand>(received)
		);
		builder.Services.AddSingleton<ICommandHandler<CreateReminderCommand, ReminderResult>>(
			new ReminderCreateStub(received)
		);
		builder.Services.AddSingleton<IValidator<UpsertInstrumentNoteCommand>, UpsertInstrumentNoteCommandValidator>();
		builder.Services.AddSingleton<IValidator<UpsertStrategyNoteCommand>, UpsertStrategyNoteCommandValidator>();
		builder.Services.AddSingleton<IValidator<UpdateReminderCommand>, UpdateReminderCommandValidator>();
		builder.Services.AddSingleton<IValidator<CreateReminderCommand>, CreateReminderCommandValidator>();
		builder.Services.Decorate(typeof(ICommandHandler<>), typeof(ValidatingCommandHandler<>));
		builder.Services.Decorate(typeof(ICommandHandler<,>), typeof(ValidatingCommandHandler<,>));
		await using var app = builder.Build();
		app.UseExceptionHandler();
		app.MapControllers();
		await app.StartAsync();
		var address = Assert.Single(
			app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses
		);
		using var client = new HttpClient { BaseAddress = new Uri(address) };
		var remindAt = new DateTime(2026, 10, 7, 12, 0, 0, DateTimeKind.Utc);
		using var request = new HttpRequestMessage(new HttpMethod(method), path)
		{
			Content = JsonContent.Create(
				new
				{
					text = "note",
					remindAt,
					userId = new { spoofed = true },
					instrumentId = new { spoofed = true },
					strategyId = new { spoofed = true },
					reminderId = new { spoofed = true },
				}
			),
		};
		using var valid = await client.SendAsync(request);
		Assert.True(valid.IsSuccessStatusCode, await valid.Content.ReadAsStringAsync());
		object expected = path switch
		{
			"/api/v1/instruments/42/note" => new UpsertInstrumentNoteCommand(7, 42, "note"),
			"/api/v1/strategies/42/note" => new UpsertStrategyNoteCommand(7, 42, "note"),
			"/api/v1/instruments/42/reminders" => new CreateReminderCommand(7, 42, "note", remindAt),
			"/api/v1/reminders/42" => new UpdateReminderCommand(7, 42, "note", remindAt),
			_ => throw new InvalidOperationException("Unexpected action."),
		};
		Assert.Equal(expected, Assert.Single(received));

		using var invalidRequest = new HttpRequestMessage(new HttpMethod(method), path)
		{
			Content = JsonContent.Create(new { text = new string('a', 1025), remindAt }),
		};
		using var invalid = await client.SendAsync(invalidRequest);
		Assert.Equal(HttpStatusCode.BadRequest, invalid.StatusCode);
		using var problem = JsonDocument.Parse(await invalid.Content.ReadAsStringAsync());
		Assert.True(problem.RootElement.GetProperty("errors").TryGetProperty("text", out _));
		Assert.Single(received);
		await app.StopAsync();
	}

	private static UpdateTradeCommand ValidTrade() =>
		new(7, 42, 1, 1, DateTime.UtcNow.AddDays(-1), null, 10, null, TradeSignal.BUY, 1);

	private static CreateTradeCommand ToCreateCommand(UpdateTradeCommand command) =>
		new(
			command.UserId,
			command.InstrumentId,
			command.TradeTypeId,
			command.OpenedAt,
			command.ClosedAt,
			command.OpenPrice,
			command.ClosePrice,
			command.Signal,
			command.Quantity
		);

	private static void AssertJsonProperties<T>(T value, params string[] expected)
	{
		using var document = JsonDocument.Parse(JsonSerializer.Serialize(value, JsonOptions));
		Assert.Equal(
			expected.Order(),
			document.RootElement.EnumerateObject().Select(property => property.Name).Order()
		);
	}

	public class JwtStub : DispatchProxy
	{
		protected override object? Invoke(MethodInfo? targetMethod, object?[]? args) =>
			targetMethod!.Name switch
			{
				nameof(IJwtHelper.GetUserIdFromClaims) => 7,
				_ => throw new InvalidOperationException($"Unexpected JWT operation: {targetMethod.Name}"),
			};
	}

	private sealed class NoteStatisticsStub : IQueryHandler<GetNoteStatisticsQuery, NoteStatisticsResult>
	{
		public GetNoteStatisticsQuery? LastQuery { get; private set; }
		public CancellationToken LastCancellationToken { get; private set; }

		public Task<NoteStatisticsResult> HandleAsync(GetNoteStatisticsQuery query, CancellationToken ct = default)
		{
			LastQuery = query;
			LastCancellationToken = ct;
			return Task.FromResult(new NoteStatisticsResult(3, 2, 1));
		}
	}

	private sealed class CreateTradeStub : ICommandHandler<CreateTradeCommand, TradeResult>
	{
		public int Calls { get; private set; }

		public Task<TradeResult> HandleAsync(CreateTradeCommand command, CancellationToken ct = default)
		{
			Calls++;
			throw new InvalidOperationException("Invalid commands must not reach this handler.");
		}
	}

	private sealed class UpdateTradeStub : ICommandHandler<UpdateTradeCommand>
	{
		public int Calls { get; private set; }
		public UpdateTradeCommand? LastCommand { get; private set; }

		public Task HandleAsync(UpdateTradeCommand command, CancellationToken ct = default)
		{
			Calls++;
			LastCommand = command;
			return Task.CompletedTask;
		}
	}

	private sealed class CommandStub<TCommand>(List<object> received) : ICommandHandler<TCommand>
		where TCommand : class, ICommand
	{
		public Task HandleAsync(TCommand command, CancellationToken ct = default)
		{
			received.Add(command);
			return Task.CompletedTask;
		}
	}

	private sealed class ReminderCreateStub(List<object> received)
		: ICommandHandler<CreateReminderCommand, ReminderResult>
	{
		public Task<ReminderResult> HandleAsync(CreateReminderCommand command, CancellationToken ct = default)
		{
			received.Add(command);
			return Task.FromResult(new ReminderResult(1, command.Text, command.RemindAt, null, null));
		}
	}

	private sealed class AuthHandlerStub<TCommand> : ICommandHandler<TCommand, AuthTokensResult>
		where TCommand : class, ICommand<AuthTokensResult>
	{
		public int Calls { get; private set; }
		public TCommand? LastCommand { get; private set; }

		public Task<AuthTokensResult> HandleAsync(TCommand command, CancellationToken ct = default)
		{
			Calls++;
			LastCommand = command;

			return Task.FromResult(
				new AuthTokensResult
				{
					AccessToken = "test-access",
					RefreshToken = "test-refresh",
					AccessTokenExpiresAt = DateTimeOffset.UtcNow.AddMinutes(1),
					RefreshTokenExpiresAt = DateTimeOffset.UtcNow.AddHours(1),
				}
			);
		}
	}

	private sealed class AuthCookieStub : IAuthCookieService
	{
		public int Calls { get; private set; }

		public void SetAuthCookies(HttpResponse response, AuthTokensResult tokens) => Calls++;

		public void DeleteAuthCookies(HttpResponse response) =>
			throw new InvalidOperationException("Unexpected cookie deletion.");
	}

	private sealed class TradesPageStub : IQueryHandler<GetTradesPageQuery, PageResult<TradeResult>>
	{
		public int Calls { get; private set; }
		public GetTradesPageQuery? LastQuery { get; private set; }

		public Task<PageResult<TradeResult>> HandleAsync(GetTradesPageQuery query, CancellationToken ct = default)
		{
			Calls++;
			LastQuery = query;
			return Task.FromResult(
				new PageResult<TradeResult>([], 0, query.PageOptions.Page, query.PageOptions.PageSize)
			);
		}
	}
}
