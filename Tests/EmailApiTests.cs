using System.Collections.Concurrent;
using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ViaTrade.Api;
using ViaTrade.Api.BackgroundServices;
using ViaTrade.Api.Controllers;
using ViaTrade.Application;
using ViaTrade.Application.Auth.Login;
using ViaTrade.Application.Email;
using ViaTrade.Configuration;
using ViaTrade.Infrastructure;
using Xunit.Abstractions;

namespace ViaTrade.Tests;

[Collection("Email integration")]
public sealed class EmailApiTests(EmailTestEnvironment environment, ITestOutputHelper output)
{
	[Fact]
	public async Task EmailChangeChecksCurrentPasswordAndResultRemainsAvailableAfterNewLogin()
	{
		await using var scenario = environment.CreateScenario();
		var user = await scenario.RegisterAsync(true);
		await using var db = environment.CreateDb();
		var tokens = await scenario
			.LoginHandler(db)
			.Handle(new LoginCommand(user.Login, "OriginalPassword!", "test"), CancellationToken.None);
		var transport = new RecordingEmailTransport();
		await using var app = await CreateApiAsync(scenario, transport);
		using var client = app.GetTestClient();
		client.DefaultRequestHeaders.Add("Cookie", "access_token=" + tokens.AccessToken);
		var email = Guid.NewGuid() + "@example.test";
		var wrong = await client.PostAsJsonAsync(
			"/api/v1/users/me/emailChange",
			new { Email = email, CurrentPassword = "WrongPassword!" }
		);
		Assert.Equal(HttpStatusCode.Unauthorized, wrong.StatusCode);
		Assert.Null((await scenario.GetUserAsync(user.Id)).PendingEmail);
		var start = await client.PostAsJsonAsync(
			"/api/v1/users/me/emailChange",
			new { Email = email, CurrentPassword = "OriginalPassword!" }
		);
		Assert.Equal(HttpStatusCode.Accepted, start.StatusCode);
		Assert.Equal(
			"succeeded",
			(await PollAsync(client, (await start.Content.ReadFromJsonAsync<EmailAcceptedResult>())!)).Status
		);
		await WaitUntilAsync(() =>
			transport.Messages.Any(message => message.UserId == user.Id && message.Kind == "EmailChange")
		);
		var changeToken = transport.Messages.Last(message => message.Kind == "EmailChange").Token;
		var confirm = await client.PostAsJsonAsync("/api/v1/users/me/emailChange/confirm", new { Token = changeToken });
		Assert.Equal(HttpStatusCode.Accepted, confirm.StatusCode);
		var accepted = (await confirm.Content.ReadFromJsonAsync<EmailAcceptedResult>())!;
		Assert.Equal("succeeded", (await PollAsync(client, accepted)).Status);
		Assert.Equal(HttpStatusCode.Forbidden, (await client.GetAsync("/api/v1/users/me")).StatusCode);
		var login = await client.PostAsJsonAsync(
			"/api/v1/sessions",
			new { Login = email, Password = "OriginalPassword!" }
		);
		Assert.Equal(HttpStatusCode.NoContent, login.StatusCode);
		var cookie = login
			.Headers.GetValues("Set-Cookie")
			.Single(value => value.StartsWith("access_token=", StringComparison.Ordinal))
			.Split(';')[0];
		client.DefaultRequestHeaders.Remove("Cookie");
		client.DefaultRequestHeaders.Add("Cookie", cookie);
		using var resultRequest = ResultRequest(accepted);
		Assert.Equal(HttpStatusCode.OK, (await client.SendAsync(resultRequest)).StatusCode);
		Assert.Equal(email, (await scenario.GetUserAsync(user.Id)).Email);
	}

	[Fact]
	public async Task ApiRegistrationAndConfirmationReturnAcceptedWithoutAutomaticLogin()
	{
		await using var scenario = environment.CreateScenario();
		var transport = new RecordingEmailTransport();
		await using var app = await CreateApiAsync(scenario, transport);
		using var client = app.GetTestClient();
		var suffix = Guid.NewGuid().ToString("N");
		var response = await client.PostAsJsonAsync(
			"/api/v1/users",
			new
			{
				Login = "api" + suffix,
				Email = suffix + "@example.test",
				Password = "OriginalPassword!",
			}
		);
		Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
		Assert.False(response.Headers.Contains("Set-Cookie"));
		var accepted = (await response.Content.ReadFromJsonAsync<EmailAcceptedResult>())!;
		Assert.Equal("succeeded", (await PollAsync(client, accepted)).Status);
		await WaitUntilAsync(() => !transport.Messages.IsEmpty);
		var confirmation = transport.Messages.Single(message => message.Kind == "Registration");
		var loginBefore = await client.PostAsJsonAsync(
			"/api/v1/sessions",
			new { Login = "api" + suffix, Password = "OriginalPassword!" }
		);
		Assert.Equal(HttpStatusCode.Forbidden, loginBefore.StatusCode);
		await client.GetAsync("/api/v1/users/emailConfirmation?token=" + confirmation.Token);
		Assert.Null((await scenario.GetUserAsync(confirmation.UserId)).EmailConfirmedAt);
		var confirm = await client.PostAsJsonAsync(
			"/api/v1/users/emailConfirmation",
			new { Token = confirmation.Token }
		);
		Assert.Equal(HttpStatusCode.Accepted, confirm.StatusCode);
		Assert.False(confirm.Headers.Contains("Set-Cookie"));
		Assert.Equal(
			"succeeded",
			(await PollAsync(client, (await confirm.Content.ReadFromJsonAsync<EmailAcceptedResult>())!)).Status
		);
		Assert.NotNull((await scenario.GetUserAsync(confirmation.UserId)).EmailConfirmedAt);
		var loginAfter = await client.PostAsJsonAsync(
			"/api/v1/sessions",
			new { Login = suffix.ToUpperInvariant() + "@EXAMPLE.TEST", Password = "OriginalPassword!" }
		);
		Assert.True(
			loginAfter.StatusCode == HttpStatusCode.NoContent,
			"Login response: " + await loginAfter.Content.ReadAsStringAsync()
		);
		Assert.True(loginAfter.Headers.Contains("Set-Cookie"));
		Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/v1/users/me")).StatusCode);
	}

	[Fact]
	public async Task PollingSuccessfulResetClearsCookiesAndOldSessionFailsAuthorization()
	{
		await using var scenario = environment.CreateScenario();
		var user = await scenario.RegisterAsync(true);
		await using var db = environment.CreateDb();
		var tokens = await scenario
			.LoginHandler(db)
			.Handle(new LoginCommand(user.Login, "OriginalPassword!", "test"), CancellationToken.None);
		var transport = new RecordingEmailTransport();
		await using var app = await CreateApiAsync(scenario, transport);
		using var client = app.GetTestClient();
		client.DefaultRequestHeaders.Add(
			"Cookie",
			"access_token=" + tokens.AccessToken + "; refresh_token=" + tokens.RefreshToken
		);
		Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/v1/users/me")).StatusCode);
		var request = await client.PostAsJsonAsync("/api/v1/users/passwordReset/request", new { Login = user.Email });
		Assert.Equal(HttpStatusCode.Accepted, request.StatusCode);
		await WaitUntilAsync(() => transport.Messages.Any(message => message.Kind == "PasswordReset"));
		var resetMessage = transport.Messages.Last(message => message.Kind == "PasswordReset");
		var resetResponse = await client.PostAsJsonAsync(
			"/api/v1/users/passwordReset",
			new
			{
				Token = resetMessage.Token,
				NewPassword = "ResetPassword!",
				UserId = user.Id + 1,
			}
		);
		Assert.Equal(HttpStatusCode.Accepted, resetResponse.StatusCode);
		var accepted = (await resetResponse.Content.ReadFromJsonAsync<EmailAcceptedResult>())!;
		Assert.True((await PollAsync(client, accepted)).ClearCookies);
		using var resultRequest = ResultRequest(accepted);
		var resultResponse = await client.SendAsync(resultRequest);
		Assert.Equal(2, resultResponse.Headers.GetValues("Set-Cookie").Count());
		Assert.All(
			resultResponse.Headers.GetValues("Set-Cookie"),
			value => Assert.Contains("expires=", value, StringComparison.OrdinalIgnoreCase)
		);
		Assert.Equal(HttpStatusCode.Forbidden, (await client.GetAsync("/api/v1/users/me")).StatusCode);
		Assert.Equal(
			HttpStatusCode.Unauthorized,
			(await client.PostAsync("/api/v1/sessions/current/tokens", null)).StatusCode
		);
	}

	[Fact]
	public async Task TwoWorkersRetryMailWithoutRepeatingAccountMutation()
	{
		await using var scenario = environment.CreateScenario();
		var transport = new RecordingEmailTransport { FailuresRemaining = 1 };
		await using var app = await CreateApiAsync(scenario, transport);
		using var second = new EmailWorkerService(
			app.Services.GetRequiredService<IServiceScopeFactory>(),
			scenario.Queue,
			scenario.Queue,
			transport,
			Options.Create(scenario.Settings),
			app.Services.GetRequiredService<ILogger<EmailWorkerService>>()
		);
		await second.StartAsync(CancellationToken.None);
		try
		{
			using var client = app.GetTestClient();
			var suffix = Guid.NewGuid().ToString("N");
			var response = await client.PostAsJsonAsync(
				"/api/v1/users",
				new
				{
					Login = "two" + suffix,
					Email = suffix + "@example.test",
					Password = "OriginalPassword!",
				}
			);
			Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
			var accepted = (await response.Content.ReadFromJsonAsync<EmailAcceptedResult>())!;
			Assert.Equal("succeeded", (await PollAsync(client, accepted)).Status);
			await WaitUntilAsync(() => transport.Messages.Count >= 2);
			Assert.Equal(transport.Messages.First().Token, transport.Messages.Last().Token);
			var user = await scenario.GetUserAsync(transport.Messages.First().UserId);
			Assert.Equal(0, user.AuthorizationVersion);
			await WaitUntilAsync(async () =>
			{
				var state = await scenario.GetUserAsync(user.Id);
				return state.EmailWorkStateJson!.Contains("AcceptedByTransportAt\":\"", StringComparison.Ordinal);
			});
		}
		finally
		{
			await second.StopAsync(CancellationToken.None);
		}
	}

	private async Task<WebApplication> CreateApiAsync(EmailScenario scenario, RecordingEmailTransport transport)
	{
		var settings = new Dictionary<string, string?>
		{
			["ConnectionStrings:MySql"] = environment.ConnectionString,
			["ConnectionStrings:Redis"] = environment.Redis.Configuration,
			["Email:RedisDatabase"] = "15",
			["Email:StreamPrefix"] = scenario.Settings.StreamPrefix,
			["Email:DataProtectionKeyPath"] = environment.KeyPath,
			["Email:IpRequestsPerMinute"] = "1000",
			["Email:TokenAttemptsPerMinute"] = "100",
			["Email:RetryBaseSeconds"] = "1",
			["Email:RetryMaxSeconds"] = "1",
			["Jwt:Secret"] = environment.JwtOptions.Value.Secret,
			["Jwt:Issuer"] = environment.JwtOptions.Value.Issuer,
			["Jwt:Audience"] = environment.JwtOptions.Value.Audience,
		};
		var configuration = new ConfigurationBuilder()
			.AddConfiguration(environment.Configuration)
			.AddInMemoryCollection(settings)
			.Build();
		var builder = WebApplication.CreateBuilder(
			new WebApplicationOptions
			{
				ApplicationName = typeof(UsersController).Assembly.FullName,
				EnvironmentName = "Development",
			}
		);
		builder.Configuration.AddConfiguration(configuration);
		builder.Logging.ClearProviders();
		builder.Logging.AddProvider(new EmailTestLoggerProvider(output));
		builder.WebHost.UseTestServer();
		builder
			.Services.AddConfigurationLayer(builder.Configuration)
			.AddApplicationLayer()
			.AddInfrastructureLayer(builder.Configuration)
			.AddApiLayer();
		builder.Services.AddSingleton<IEmailQueue>(scenario.Queue);
		builder.Services.AddSingleton<IEmailStreamWorker>(scenario.Queue);
		builder.Services.AddSingleton<IEmailTransport>(transport);
		builder.Services.AddHostedService<EmailWorkerService>();
		var app = builder.Build();
		app.UseExceptionHandler();
		app.UseAuthentication();
		app.UseAuthorization();
		app.MapControllers();
		await app.StartAsync();
		return app;
	}

	private static HttpRequestMessage ResultRequest(EmailAcceptedResult accepted)
	{
		var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/users/operations/" + accepted.OperationId);
		request.Headers.Add("X-Operation-Key", accepted.ResultAccessKey);
		return request;
	}

	private static async Task<EmailOperationResult> PollAsync(HttpClient client, EmailAcceptedResult accepted)
	{
		EmailOperationResult? result = null;
		await WaitUntilAsync(async () =>
		{
			using var request = ResultRequest(accepted);
			var response = await client.SendAsync(request);
			Assert.Equal(HttpStatusCode.OK, response.StatusCode);
			result = await response.Content.ReadFromJsonAsync<EmailOperationResult>();
			return result!.Status != "pending";
		});
		return result!;
	}

	private static Task WaitUntilAsync(Func<bool> predicate) => WaitUntilAsync(() => Task.FromResult(predicate()));

	private static async Task WaitUntilAsync(Func<Task<bool>> predicate)
	{
		using var deadline = new CancellationTokenSource(TimeSpan.FromSeconds(15));
		while (!await predicate())
			await Task.Delay(50, deadline.Token);
	}
}

public sealed class RecordingEmailTransport : IEmailTransport
{
	public ConcurrentQueue<EmailMessage> Messages { get; } = new();
	public int FailuresRemaining;

	public Task SendAsync(EmailMessage message, CancellationToken ct)
	{
		Messages.Enqueue(message);
		if (Interlocked.Decrement(ref FailuresRemaining) >= 0)
			return Task.FromException(new IOException("Injected SMTP failure."));
		return Task.CompletedTask;
	}
}

public sealed class EmailTestLoggerProvider(ITestOutputHelper output) : ILoggerProvider
{
	public ILogger CreateLogger(string categoryName) => new EmailTestLogger(output, categoryName);

	public void Dispose() { }

	private sealed class EmailTestLogger(ITestOutputHelper output, string category) : ILogger
	{
		public IDisposable? BeginScope<TState>(TState state)
			where TState : notnull => null;

		public bool IsEnabled(LogLevel logLevel) =>
			category.EndsWith(nameof(EmailWorkerService), StringComparison.Ordinal);

		public void Log<TState>(
			LogLevel logLevel,
			EventId eventId,
			TState state,
			Exception? exception,
			Func<TState, Exception?, string> formatter
		)
		{
			if (IsEnabled(logLevel))
				output.WriteLine(formatter(state, exception));
		}
	}
}
