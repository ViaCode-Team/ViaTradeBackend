using System.Net;
using System.Net.Sockets;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using ViaTrade.Application.Common.Exceptions;
using ViaTrade.Application.Email;
using ViaTrade.Infrastructure.Email;

namespace ViaTrade.Tests;

[Collection("Email integration")]
public sealed class EmailQueueTests(EmailTestEnvironment environment)
{
	[Fact]
	public async Task DisconnectedRedisDoesNotAcceptJob()
	{
		var listener = new TcpListener(IPAddress.Loopback, 0);
		listener.Start();
		var port = ((IPEndPoint)listener.LocalEndpoint).Port;
		listener.Stop();
		var connectionOptions = new ConfigurationOptions
		{
			AbortOnConnectFail = false,
			ConnectRetry = 0,
			ConnectTimeout = 100,
			AsyncTimeout = 100,
			SyncTimeout = 100,
		};
		connectionOptions.EndPoints.Add("127.0.0.1", port);
		await using var connection = await ConnectionMultiplexer.ConnectAsync(connectionOptions);
		await using var scenario = environment.CreateScenario();
		var queue = new RedisEmailQueue(connection, environment.Protection, Options.Create(scenario.Settings));
		await Assert.ThrowsAsync<ServiceUnavailableException>(() =>
			queue.PublishAsync(
				scenario.Job(EmailAction.ResendConfirmation) with
				{
					Login = "offline@example.test",
				},
				CancellationToken.None
			)
		);
		Assert.Equal(0, await scenario.Database.StreamLengthAsync(scenario.Settings.StreamPrefix + ":actions"));
	}

	[Fact]
	public async Task SuccessfulResultCannotBeOverwrittenByLateFailure()
	{
		await using var scenario = environment.CreateScenario();
		var job = scenario.Job(EmailAction.ConfirmRegistration);
		var accepted = await scenario.Queue.PublishAsync(job, CancellationToken.None);
		var success = new EmailOperationResult("succeeded", "email_confirmed", "Confirmed.");
		await scenario.Queue.SetResultAsync(job.Id, success, CancellationToken.None);
		await scenario.Queue.SetResultAsync(
			job.Id,
			new EmailOperationResult("failed", "processing_expired", "Expired."),
			CancellationToken.None
		);
		Assert.Equal(
			success,
			await scenario.Queue.FindResultAsync(null, job.Id, null, accepted.ResultAccessKey, CancellationToken.None)
		);
		Assert.Equal(success, await scenario.Queue.FindTerminalResultAsync(job.Id, CancellationToken.None));
	}

	[Fact]
	public async Task PublicResultRequiresSecretAndQueueDoesNotStorePlainPasswordOrHash()
	{
		await using var scenario = environment.CreateScenario();
		var hash = environment.Hasher.Hash("OriginalPassword!");
		var job = scenario.Job(EmailAction.Register) with
		{
			Login = "queue-test",
			Email = "queue@example.test",
			PasswordHash = hash,
		};
		var accepted = await scenario.Queue.PublishAsync(job, CancellationToken.None);
		Assert.Null(
			await scenario.Queue.FindResultAsync(null, job.Id, null, EmailIdentity.NewSecret(), CancellationToken.None)
		);
		Assert.Equal(
			"pending",
			(
				await scenario.Queue.FindResultAsync(
					null,
					job.Id,
					null,
					accepted.ResultAccessKey,
					CancellationToken.None
				)
			)!.Status
		);
		var entry = Assert.Single(
			await scenario.Database.StreamRangeAsync(scenario.Settings.StreamPrefix + ":actions")
		);
		var payload = entry.Values.Single(value => value.Name == "payload").Value.ToString();
		Assert.DoesNotContain("OriginalPassword!", payload);
		Assert.DoesNotContain(hash, payload);
		Assert.DoesNotContain(job.Email, payload);
		await scenario.Queue.InitializeAsync(CancellationToken.None);
		var decoded = Assert.Single(await scenario.Queue.ReadAsync(false, "test", CancellationToken.None));
		Assert.Equal(hash, decoded.Job!.PasswordHash);
		Assert.Equal(job.Id, decoded.Job.Id);
	}

	[Fact]
	public async Task AuthorizedResultRequiresSameAccountAndSecret()
	{
		await using var scenario = environment.CreateScenario();
		var job = scenario.Job(EmailAction.StartEmailChange) with
		{
			UserId = 123,
			SessionId = "owner-session",
			Email = "new@example.test",
			VerifiedAuthorizationVersion = 4,
		};
		var accepted = await scenario.Queue.PublishAsync(job, CancellationToken.None);
		Assert.Null(
			await scenario.Queue.FindResultAsync(null, job.Id, null, accepted.ResultAccessKey, CancellationToken.None)
		);
		Assert.Null(
			await scenario.Queue.FindResultAsync(
				124,
				job.Id,
				"owner-session",
				accepted.ResultAccessKey,
				CancellationToken.None
			)
		);
		Assert.NotNull(
			await scenario.Queue.FindResultAsync(
				123,
				job.Id,
				"new-owner-session",
				accepted.ResultAccessKey,
				CancellationToken.None
			)
		);
		Assert.NotNull(
			await scenario.Queue.FindResultAsync(
				123,
				job.Id,
				"owner-session",
				accepted.ResultAccessKey,
				CancellationToken.None
			)
		);
	}

	[Fact]
	public async Task ResetAndResendResultsDoNotExposeAccountOrWorkerOutcome()
	{
		await using var scenario = environment.CreateScenario();
		foreach (var action in new[] { EmailAction.RequestPasswordReset, EmailAction.ResendConfirmation })
		{
			var first = await scenario.Queue.PublishAsync(
				scenario.Job(action) with
				{
					Login = "present@example.test",
				},
				CancellationToken.None
			);
			var second = await scenario.Queue.PublishAsync(
				scenario.Job(action) with
				{
					Login = "missing@example.test",
				},
				CancellationToken.None
			);
			await scenario.Queue.SetResultAsync(
				first.OperationId,
				new EmailOperationResult("succeeded", "account_found", "found"),
				CancellationToken.None
			);
			await scenario.Queue.SetResultAsync(
				second.OperationId,
				new EmailOperationResult("failed", "account_missing", "missing"),
				CancellationToken.None
			);
			Assert.Equal(
				EmailWorkflowService.PrivateResult,
				await scenario.Queue.FindResultAsync(
					null,
					first.OperationId,
					null,
					first.ResultAccessKey,
					CancellationToken.None
				)
			);
			Assert.Equal(
				EmailWorkflowService.PrivateResult,
				await scenario.Queue.FindResultAsync(
					null,
					second.OperationId,
					null,
					second.ResultAccessKey,
					CancellationToken.None
				)
			);
		}
	}

	[Fact]
	public async Task LimitsAndCooldownAreSharedByInstancesAndAtomic()
	{
		await using var scenario = environment.CreateScenario();
		var other = new RedisEmailQueue(environment.Redis, environment.Protection, Options.Create(scenario.Settings));
		var requests = Enumerable
			.Range(0, 50)
			.Select(index =>
				other.TryAcquireLimitAsync("test", "ip", 5, TimeSpan.FromMinutes(1), CancellationToken.None)
			);
		var results = await Task.WhenAll(requests);
		Assert.Equal(5, results.Count(allowed => allowed));
		Assert.False(
			await scenario.Queue.TryAcquireLimitAsync("test", "ip", 5, TimeSpan.FromMinutes(1), CancellationToken.None)
		);
		var id = Guid.NewGuid();
		Assert.True(
			await scenario.Queue.TryAcquireMailCooldownAsync(
				"A@EXAMPLE.TEST",
				EmailAction.Register,
				id,
				CancellationToken.None
			)
		);
		Assert.True(
			await other.TryAcquireMailCooldownAsync("A@EXAMPLE.TEST", EmailAction.Register, id, CancellationToken.None)
		);
		Assert.False(
			await other.TryAcquireMailCooldownAsync(
				"A@EXAMPLE.TEST",
				EmailAction.ResendConfirmation,
				Guid.NewGuid(),
				CancellationToken.None
			)
		);
		var ttl = await scenario.Database.KeyTimeToLiveAsync(
			scenario.Settings.StreamPrefix + ":cooldown:registration:" + EmailIdentity.Fingerprint("A@EXAMPLE.TEST")
		);
		Assert.InRange(ttl!.Value, TimeSpan.FromMinutes(4), TimeSpan.FromMinutes(5));
	}

	[Fact]
	public async Task MailPublicationIsIdempotentAndRetryKeepsSameLink()
	{
		await using var scenario = environment.CreateScenario();
		var intent = new EmailIntent
		{
			Id = Guid.NewGuid(),
			Kind = "Registration",
			Recipient = "mail@example.test",
			ExpiresAt = EmailIdentity.UtcNow().AddHours(24),
			Token = new EmailToken(
				EmailTokenPurpose.Registration,
				123,
				Guid.NewGuid(),
				0,
				EmailIdentity.UtcNow().AddHours(24)
			),
		};
		await scenario.Queue.PublishMailAsync(123, intent, CancellationToken.None);
		var protectedBefore = await scenario.Database.StringGetAsync(
			scenario.Settings.StreamPrefix + ":event:" + intent.Id.ToString("N")
		);
		await scenario.Queue.PublishMailAsync(123, intent, CancellationToken.None);
		Assert.Equal(
			protectedBefore,
			await scenario.Database.StringGetAsync(scenario.Settings.StreamPrefix + ":event:" + intent.Id.ToString("N"))
		);
		Assert.Equal(1, await scenario.Database.StreamLengthAsync(scenario.Settings.StreamPrefix + ":mail"));
		await scenario.Queue.InitializeAsync(CancellationToken.None);
		var item = Assert.Single(await scenario.Queue.ReadAsync(true, "smtp", CancellationToken.None));
		Assert.NotNull(
			await scenario.Queue.FindTokenAsync(EmailIdentity.Fingerprint(item.Message!.Token!), CancellationToken.None)
		);
		Assert.DoesNotContain(item.Message.Token!, protectedBefore.ToString());
		Assert.Equal(1, await scenario.Queue.TryLeaseAsync(true, item.StreamId, "smtp", CancellationToken.None));
		await scenario.Queue.FailAsync(true, item with { Attempt = 1 }, "smtp", CancellationToken.None);
		Assert.Equal(0, await scenario.Queue.TryLeaseAsync(true, item.StreamId, "smtp", CancellationToken.None));
		await scenario.Database.HashSetAsync(scenario.Settings.StreamPrefix + ":meta:True:" + item.StreamId, "due", 0);
		var retry = Assert.Single(await scenario.Queue.ReadAsync(true, "smtp", CancellationToken.None));
		Assert.Equal(item.Message.Token, retry.Message!.Token);
		Assert.Equal(2, await scenario.Queue.TryLeaseAsync(true, retry.StreamId, "smtp", CancellationToken.None));
		await scenario.Queue.CompleteAsync(true, retry, "smtp", CancellationToken.None);
		Assert.Equal(0, await scenario.Database.StreamLengthAsync(scenario.Settings.StreamPrefix + ":mail"));
	}

	[Fact]
	public async Task AnotherConsumerClaimsAbandonedMessageAndLeasePreventsConcurrentExecution()
	{
		await using var scenario = environment.CreateScenario();
		await scenario.Queue.InitializeAsync(CancellationToken.None);
		await scenario.Queue.PublishAsync(
			scenario.Job(EmailAction.ResendConfirmation) with
			{
				Login = "test@example.test",
			},
			CancellationToken.None
		);
		var first = Assert.Single(await scenario.Queue.ReadAsync(false, "crashed", CancellationToken.None));
		await scenario.Database.ExecuteAsync(
			"XCLAIM",
			scenario.Settings.StreamPrefix + ":actions",
			"workers",
			"crashed",
			0,
			first.StreamId,
			"IDLE",
			61000
		);
		var second = Assert.Single(await scenario.Queue.ReadAsync(false, "replacement", CancellationToken.None));
		Assert.Equal(first.Job!.Id, second.Job!.Id);
		Assert.Equal(
			1,
			await scenario.Queue.TryLeaseAsync(false, second.StreamId, "replacement", CancellationToken.None)
		);
		Assert.Equal(0, await scenario.Queue.TryLeaseAsync(false, first.StreamId, "crashed", CancellationToken.None));
		await scenario.Queue.CompleteAsync(false, second, "replacement", CancellationToken.None);
		Assert.Equal(
			0,
			(
				await scenario.Database.StreamPendingAsync(scenario.Settings.StreamPrefix + ":actions", "workers")
			).PendingMessageCount
		);
	}

	[Fact]
	public async Task FinalFailureIsProtectedAndRetainedSeparately()
	{
		await using var scenario = environment.CreateScenario();
		await scenario.Queue.InitializeAsync(CancellationToken.None);
		await scenario.Queue.PublishAsync(
			scenario.Job(EmailAction.ResendConfirmation) with
			{
				Login = "failed@example.test",
			},
			CancellationToken.None
		);
		var item = Assert.Single(await scenario.Queue.ReadAsync(false, "test", CancellationToken.None));
		await scenario.Queue.TryLeaseAsync(false, item.StreamId, "test", CancellationToken.None);
		await scenario.Queue.FailAsync(
			false,
			item with
			{
				Attempt = scenario.Settings.MaxAttempts,
			},
			"test",
			CancellationToken.None
		);
		var key = scenario.Settings.StreamPrefix + ":dead:False:" + item.StreamId;
		var failure = await scenario.Database.StringGetAsync(key);
		Assert.False(failure.IsNullOrEmpty);
		Assert.DoesNotContain(item.Job!.Login!, failure.ToString());
		Assert.True(await scenario.Database.KeyTimeToLiveAsync(key) <= TimeSpan.FromDays(7));
		Assert.Equal(0, await scenario.Database.StreamLengthAsync(scenario.Settings.StreamPrefix + ":actions"));
	}

	[Fact]
	public async Task CapacityRejectsNewRequestsWithoutTrimmingUnprocessedMessages()
	{
		await using var scenario = environment.CreateScenario();
		scenario.Settings.MaxQueuedMessages = 2;
		await scenario.Queue.PublishAsync(
			scenario.Job(EmailAction.ResendConfirmation) with
			{
				Login = "first@example.test",
			},
			CancellationToken.None
		);
		await scenario.Queue.PublishAsync(
			scenario.Job(EmailAction.ResendConfirmation) with
			{
				Login = "second@example.test",
			},
			CancellationToken.None
		);
		await Assert.ThrowsAsync<ServiceUnavailableException>(() =>
			scenario.Queue.PublishAsync(
				scenario.Job(EmailAction.ResendConfirmation) with
				{
					Login = "third@example.test",
				},
				CancellationToken.None
			)
		);
		Assert.Equal(2, await scenario.Database.StreamLengthAsync(scenario.Settings.StreamPrefix + ":actions"));
	}

	[Fact]
	public async Task PersistentKeysAllowAnotherProviderToReadAfterRestart()
	{
		await using var scenario = environment.CreateScenario();
		await scenario.Queue.PublishAsync(
			scenario.Job(EmailAction.ResendConfirmation) with
			{
				Login = "restart@example.test",
			},
			CancellationToken.None
		);
		var restartedProtection = DataProtectionProvider.Create(
			new DirectoryInfo(environment.KeyPath),
			builder => builder.SetApplicationName("ViaTrade.Email.Tests")
		);
		var restarted = new RedisEmailQueue(environment.Redis, restartedProtection, Options.Create(scenario.Settings));
		await restarted.InitializeAsync(CancellationToken.None);
		Assert.Equal(
			"restart@example.test",
			Assert.Single(await restarted.ReadAsync(false, "restarted", CancellationToken.None)).Job!.Login
		);
	}

	[Fact]
	public async Task MalformedPayloadIsQuarantinedAndDoesNotBlockStream()
	{
		await using var scenario = environment.CreateScenario();
		await scenario.Queue.InitializeAsync(CancellationToken.None);
		var badId = await scenario.Database.StreamAddAsync(
			scenario.Settings.StreamPrefix + ":actions",
			[new NameValueEntry("payload", "invalid")]
		);
		await scenario.Queue.PublishAsync(
			scenario.Job(EmailAction.ResendConfirmation) with
			{
				Login = "valid@example.test",
			},
			CancellationToken.None
		);
		Assert.Equal(
			"valid@example.test",
			Assert.Single(await scenario.Queue.ReadAsync(false, "test", CancellationToken.None)).Job!.Login
		);
		Assert.True(await scenario.Database.KeyExistsAsync(scenario.Settings.StreamPrefix + ":dead:False:" + badId));
	}
}
