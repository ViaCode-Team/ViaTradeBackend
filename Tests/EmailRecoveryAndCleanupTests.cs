using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using ViaTrade.Application.Auth.Common;
using ViaTrade.Application.Auth.Login;
using ViaTrade.Application.Common.Exceptions;
using ViaTrade.Application.Email;
using ViaTrade.Domain.Entities;
using ViaTrade.Infrastructure.DataBase.Repositories.Generic;
using ViaTrade.Infrastructure.Email;

namespace ViaTrade.Tests;

[Collection("Email integration")]
public sealed class EmailRecoveryAndCleanupTests(EmailTestEnvironment environment)
{
	[Fact]
	public async Task MissingEmailCannotBeTreatedAsConfirmed()
	{
		await using var scenario = environment.CreateScenario();
		var user = await scenario.RegisterAsync(true);
		var session = await scenario.CreateSessionAsync(user);
		await using var db = environment.CreateDb();
		await db
			.Users.Where(account => account.Id == user.Id)
			.ExecuteUpdateAsync(setters =>
				setters
					.SetProperty(account => account.Email, (string?)null)
					.SetProperty(account => account.NormalizedEmail, (string?)null)
			);
		await Assert.ThrowsAsync<ForbiddenException>(() =>
			scenario
				.LoginHandler(db)
				.Handle(new LoginCommand(user.Login, "OriginalPassword!", "test"), CancellationToken.None)
				.AsTask()
		);
		Assert.False(await new AccountSessionValidator(new ReadEfRepository<User>(db)).IsCurrentAsync(session));
	}

	[Fact]
	public async Task NamedLockIsReleasedOnFailureAndCancellation()
	{
		await using var db = environment.CreateDb();
		var store = new EmailAccountStore(db);
		await Assert.ThrowsAsync<BusinessRuleException>(() =>
			store.ExecuteAsync<bool>(_ => throw new BusinessRuleException("Injected failure."), CancellationToken.None)
		);
		using var cancellation = new CancellationTokenSource();
		await Assert.ThrowsAsync<OperationCanceledException>(() =>
			store.ExecuteAsync<bool>(
				_ =>
				{
					cancellation.Cancel();
					cancellation.Token.ThrowIfCancellationRequested();
					return Task.FromResult(false);
				},
				cancellation.Token
			)
		);
		await using var connection = new MySqlConnection(environment.ConnectionString);
		await connection.OpenAsync();
		await using var command = connection.CreateCommand();
		command.CommandText = "SELECT IS_FREE_LOCK(@name)";
		command.Parameters.AddWithValue(
			"@name",
			"viatrade:email:" + EmailIdentity.Fingerprint(connection.Database)[..40]
		);
		Assert.Equal(1, Convert.ToInt32(await command.ExecuteScalarAsync()));
	}

	[Fact]
	public async Task ExpiredRecoveryReservationIsCleanedAndAddressBecomesAvailable()
	{
		await using var scenario = environment.CreateScenario();
		var original = await scenario.RegisterAsync(true);
		var (changed, recovery) = await scenario.ChangeEmailAsync(original);
		await using var db = environment.CreateDb();
		await db
			.Users.Where(user => user.Id == changed.Id)
			.ExecuteUpdateAsync(setters =>
				setters.SetProperty(user => user.EmailRecoveryExpiresAt, EmailIdentity.UtcNow().AddSeconds(-1))
			);
		var workflow = new EmailWorkflowService(new EmailAccountStore(db), scenario.Queue, environment.Sessions);
		await workflow.CleanupAsync(changed.Id, CancellationToken.None);
		var cleaned = await scenario.GetUserAsync(changed.Id);
		Assert.Null(cleaned.PreviousEmail);
		Assert.Null(cleaned.NormalizedPreviousEmail);
		Assert.Null(cleaned.CompletedEmailChangeId);
		Assert.Equal(
			"failed",
			(
				await scenario.ProcessAsync(
					scenario.TokenJob(EmailAction.RecoverEmail, recovery, environment.Hasher.Hash("RecoveryPassword!"))
				)
			).Status
		);
		await scenario.ClearCooldownAsync(original.Email!, "registration");
		Assert.NotNull(await scenario.RegisterAsync(email: original.Email));
	}

	[Fact]
	public async Task PendingChangeResendPreservesOperationAndDeadline()
	{
		await using var scenario = environment.CreateScenario();
		var user = await scenario.RegisterAsync(true);
		var session = await scenario.CreateSessionAsync(user);
		var email = Guid.NewGuid() + "@example.test";
		var start = scenario.Job(EmailAction.StartEmailChange) with
		{
			UserId = user.Id,
			SessionId = session.Id,
			Email = email,
			VerifiedAuthorizationVersion = user.AuthorizationVersion,
		};
		await scenario.ProcessAsync(start);
		await scenario.RecoverAsync(user.Id);
		var first = scenario.LastMessage(user.Id, "EmailChange");
		var initial = await scenario.GetUserAsync(user.Id);
		await scenario.ClearCooldownAsync(email, "emailChange");
		await scenario.ProcessAsync(
			start with
			{
				Id = Guid.NewGuid(),
				CreatedAt = EmailIdentity.UtcNow().AddMinutes(1),
			}
		);
		await scenario.RecoverAsync(user.Id);
		var second = scenario.LastMessage(user.Id, "EmailChange");
		Assert.NotEqual(first.Token, second.Token);
		Assert.Equal(first.ExpiresAt, second.ExpiresAt);
		Assert.Equal(initial.EmailChangeOperationId, (await scenario.GetUserAsync(user.Id)).EmailChangeOperationId);
		Assert.Equal(user.Email, (await scenario.GetUserAsync(user.Id)).Email);
	}

	[Fact]
	public async Task SessionRemovalIsBoundedAndPreservesNewAuthorizationVersion()
	{
		await using var scenario = environment.CreateScenario();
		var user = await scenario.RegisterAsync(true);
		for (var index = 0; index < 105; index++)
			await scenario.CreateSessionAsync(user);
		await scenario.ProcessAsync(scenario.Job(EmailAction.RequestPasswordReset) with { Login = user.Email });
		await scenario.RecoverAsync(user.Id);
		await scenario.ProcessAsync(
			scenario.TokenJob(
				EmailAction.ResetPassword,
				scenario.LastMessage(user.Id, "PasswordReset"),
				environment.Hasher.Hash("ResetPassword!")
			)
		);
		var changed = await scenario.GetUserAsync(user.Id);
		var current = await scenario.CreateSessionAsync(changed);
		await scenario.RecoverAsync(user.Id);
		Assert.Equal(6, (await environment.Sessions.ListByUserAsync(user.Id)).Count);
		await scenario.RecoverAsync(user.Id);
		Assert.Equal(current.Id, Assert.Single(await environment.Sessions.ListByUserAsync(user.Id)).Id);
		Assert.Null(
			JsonSerializer
				.Deserialize<EmailWorkState>((await scenario.GetUserAsync(user.Id)).EmailWorkStateJson!)!
				.RevokeSessionsBeforeVersion
		);
	}

	[Fact]
	public async Task AcceptedTransportOutcomeReconcilesDatabaseAfterPartialFailure()
	{
		await using var scenario = environment.CreateScenario();
		var user = await scenario.RegisterAsync();
		var message = scenario.LastMessage(user.Id, "Registration");
		await scenario.Queue.SetMailOutcomeAsync(message, true, CancellationToken.None);
		await scenario.RecoverAsync(user.Id);
		var work = JsonSerializer.Deserialize<EmailWorkState>(
			(await scenario.GetUserAsync(user.Id)).EmailWorkStateJson!
		)!;
		Assert.NotNull(work.Intents.Single(intent => intent.Id == message.EventId).AcceptedByTransportAt);
		Assert.Equal(
			"acceptedByTransport",
			await scenario.Queue.FindMailOutcomeAsync(message.EventId, CancellationToken.None)
		);
	}

	[Fact]
	public async Task RecoveryReissuesLostPendingTokenWithoutExtendingDeadline()
	{
		await using var scenario = environment.CreateScenario();
		var original = await scenario.RegisterAsync(true);
		var (_, first) = await scenario.ChangeEmailAsync(original);
		await scenario.Database.KeyDeleteAsync(
			scenario.Settings.StreamPrefix + ":token:" + EmailIdentity.Fingerprint(first.Token!)
		);
		await scenario.Database.KeyDeleteAsync(
			scenario.Settings.StreamPrefix + ":event:" + first.EventId.ToString("N")
		);
		await scenario.RecoverAsync(original.Id);
		var second = scenario.LastMessage(original.Id, "EmailRecovery");
		Assert.NotEqual(first.Token, second.Token);
		Assert.Equal(first.ExpiresAt, second.ExpiresAt);
		Assert.Equal(
			"succeeded",
			(
				await scenario.ProcessAsync(
					scenario.TokenJob(EmailAction.RecoverEmail, second, environment.Hasher.Hash("RecoveryPassword!"))
				)
			).Status
		);
	}
}
