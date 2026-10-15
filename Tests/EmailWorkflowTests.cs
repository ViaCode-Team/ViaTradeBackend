using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using ViaTrade.Application.Auth.Common;
using ViaTrade.Application.Auth.Common.Abstractions;
using ViaTrade.Application.Auth.Login;
using ViaTrade.Application.Auth.RefreshTokens;
using ViaTrade.Application.Auth.Register;
using ViaTrade.Application.Common.Exceptions;
using ViaTrade.Application.Email;
using ViaTrade.Domain.Entities;
using ViaTrade.Infrastructure.DataBase.Repositories.Generic;
using ViaTrade.Infrastructure.Email;
using ViaTrade.Infrastructure.Services;
using Xunit.Abstractions;

namespace ViaTrade.Tests;

[Collection("Email integration")]
public sealed class EmailWorkflowTests(EmailTestEnvironment environment, ITestOutputHelper output)
{
	[Fact]
	public async Task RegistrationRequiresConfirmationAndLoginAcceptsNormalizedLoginAndEmail()
	{
		output.WriteLine("MySQL: " + environment.MySqlVersion + "; Redis: " + environment.RedisVersion);
		await using var scenario = environment.CreateScenario();
		var user = await scenario.RegisterAsync();
		await using var db = environment.CreateDb();
		var login = scenario.LoginHandler(db);
		await Assert.ThrowsAsync<InvalidCredentialsException>(() =>
			login.Handle(new LoginCommand(user.Login, "WrongPassword!", "test"), CancellationToken.None).AsTask()
		);
		await Assert.ThrowsAsync<ForbiddenException>(() =>
			login.Handle(new LoginCommand(user.Login, "OriginalPassword!", "test"), CancellationToken.None).AsTask()
		);
		var confirmation = scenario.LastMessage(user.Id, "Registration");
		Assert.Equal(
			"succeeded",
			(await scenario.ProcessAsync(scenario.TokenJob(EmailAction.ConfirmRegistration, confirmation))).Status
		);
		Assert.NotNull((await scenario.GetUserAsync(user.Id)).EmailConfirmedAt);
		Assert.NotNull(
			await login.Handle(
				new LoginCommand(user.Login.ToUpperInvariant(), "OriginalPassword!", "test"),
				CancellationToken.None
			)
		);
		Assert.NotNull(
			await login.Handle(
				new LoginCommand(user.Email!.ToUpperInvariant(), "OriginalPassword!", "test"),
				CancellationToken.None
			)
		);
		Assert.Equal(
			"failed",
			(await scenario.ProcessAsync(scenario.TokenJob(EmailAction.ConfirmRegistration, confirmation))).Status
		);
	}

	[Fact]
	public async Task DuplicateRegistrationDoesNotOverwriteCredentials()
	{
		await using var scenario = environment.CreateScenario();
		var user = await scenario.RegisterAsync();
		var duplicate = scenario.Job(EmailAction.Register) with
		{
			Login = user.Login.ToUpperInvariant(),
			Email = Guid.NewGuid() + "@example.test",
			PasswordHash = environment.Hasher.Hash("ReplacementPassword!"),
		};
		Assert.Equal("identity_unavailable", (await scenario.ProcessAsync(duplicate)).Code);
		var unchanged = await scenario.GetUserAsync(user.Id);
		Assert.Equal(user.PasswordHash, unchanged.PasswordHash);
		Assert.Equal(user.Email, unchanged.Email);
		Assert.Equal(user.Login, unchanged.Login);
	}

	[Fact]
	public async Task ResendPreservesDeadlineAndAllowsOnlyOneOfDifferentLinks()
	{
		await using var scenario = environment.CreateScenario();
		var user = await scenario.RegisterAsync();
		var first = scenario.LastMessage(user.Id, "Registration");
		var request = scenario.Job(EmailAction.ResendConfirmation) with { Login = user.Email };
		Assert.Equal(EmailWorkflowService.PrivateResult, await scenario.ProcessAsync(request));
		await scenario.RecoverAsync(user.Id);
		Assert.Single(scenario.Messages, message => message.UserId == user.Id);
		await scenario.ClearCooldownAsync(user.Email!, "registration");
		Assert.Equal(
			EmailWorkflowService.PrivateResult,
			await scenario.ProcessAsync(request with { Id = Guid.NewGuid() })
		);
		await scenario.RecoverAsync(user.Id);
		var second = scenario.LastMessage(user.Id, "Registration");
		Assert.NotEqual(first.Token, second.Token);
		Assert.Equal(first.ExpiresAt, second.ExpiresAt);
		var results = await Task.WhenAll(
			scenario.ProcessAsync(scenario.TokenJob(EmailAction.ConfirmRegistration, first)),
			scenario.ProcessAsync(scenario.TokenJob(EmailAction.ConfirmRegistration, second))
		);
		Assert.Single(results, result => result.Status == "succeeded");
		Assert.Single(results, result => result.Status == "failed");
	}

	[Theory]
	[InlineData(EmailTokenPurpose.PasswordReset, false)]
	[InlineData(EmailTokenPurpose.Registration, true)]
	public async Task WrongPurposeOrExpiredTokenDoesNotConfirm(EmailTokenPurpose purpose, bool expired)
	{
		await using var scenario = environment.CreateScenario();
		var user = await scenario.RegisterAsync();
		var expires = user.EmailConfirmationExpiresAt!.Value;
		if (expired)
			expires = EmailIdentity.UtcNow().AddSeconds(-1);
		var secret = await scenario.StoreTokenAsync(
			new EmailToken(
				purpose,
				user.Id,
				user.EmailConfirmationOperationId!.Value,
				user.AuthorizationVersion,
				expires
			)
		);
		var job = scenario.Job(EmailAction.ConfirmRegistration) with
		{
			UserId = user.Id,
			TokenFingerprint = EmailIdentity.Fingerprint(secret),
		};
		Assert.Equal("failed", (await scenario.ProcessAsync(job)).Status);
		Assert.Null((await scenario.GetUserAsync(user.Id)).EmailConfirmedAt);
	}

	[Fact]
	public async Task ResetRevokesSessionsCancelsPendingEmailAndDoesNotConfirmUnconfirmedAccount()
	{
		await using var scenario = environment.CreateScenario();
		var user = await scenario.RegisterAsync();
		var originalConfirmation = scenario.LastMessage(user.Id, "Registration");
		var session = await scenario.CreateSessionAsync(user);
		Assert.Equal(
			EmailWorkflowService.PrivateResult,
			await scenario.ProcessAsync(scenario.Job(EmailAction.RequestPasswordReset) with { Login = user.Login })
		);
		await scenario.RecoverAsync(user.Id);
		var reset = scenario.LastMessage(user.Id, "PasswordReset");
		var hash = environment.Hasher.Hash("ResetPassword!");
		var job = scenario.TokenJob(EmailAction.ResetPassword, reset, hash);
		var outcome = await scenario.ProcessAsync(job);
		Assert.True(outcome.ClearCookies);
		var changed = await scenario.GetUserAsync(user.Id);
		Assert.Null(changed.EmailConfirmedAt);
		Assert.Equal(1, changed.AuthorizationVersion);
		Assert.Equal(hash, changed.PasswordHash);
		Assert.Equal(
			"failed",
			(
				await scenario.ProcessAsync(scenario.TokenJob(EmailAction.ConfirmRegistration, originalConfirmation))
			).Status
		);
		await using var db = environment.CreateDb();
		Assert.False(await new AccountSessionValidator(new ReadEfRepository<User>(db)).IsCurrentAsync(session));
		await scenario.RecoverAsync(user.Id);
		Assert.Null(await environment.Sessions.FindByIdAsync(session.Id));
		Assert.Equal(
			"succeeded",
			(
				await scenario.ProcessAsync(
					scenario.TokenJob(EmailAction.ConfirmRegistration, scenario.LastMessage(user.Id, "Registration"))
				)
			).Status
		);
		Assert.Equal(outcome, await scenario.ProcessAsync(job));
		Assert.Equal(1, (await scenario.GetUserAsync(user.Id)).AuthorizationVersion);
	}

	[Fact]
	public async Task ConcurrentResetLinksApplyOnlyOnePasswordChange()
	{
		await using var scenario = environment.CreateScenario();
		var user = await scenario.RegisterAsync(true);
		var request = scenario.Job(EmailAction.RequestPasswordReset) with { Login = user.Login };
		await scenario.ProcessAsync(request);
		await scenario.RecoverAsync(user.Id);
		var first = scenario.LastMessage(user.Id, "PasswordReset");
		await scenario.ClearCooldownAsync(user.Email!, "passwordReset");
		await scenario.ProcessAsync(
			request with
			{
				Id = Guid.NewGuid(),
				CreatedAt = EmailIdentity.UtcNow().AddMinutes(1),
			}
		);
		await scenario.RecoverAsync(user.Id);
		var second = scenario.LastMessage(user.Id, "PasswordReset");
		Assert.Equal(first.ExpiresAt, second.ExpiresAt);
		var results = await Task.WhenAll(
			scenario.ProcessAsync(
				scenario.TokenJob(EmailAction.ResetPassword, first, environment.Hasher.Hash("FirstPassword!"))
			),
			scenario.ProcessAsync(
				scenario.TokenJob(EmailAction.ResetPassword, second, environment.Hasher.Hash("SecondPassword!"))
			)
		);
		Assert.Single(results, result => result.Status == "succeeded");
		Assert.Equal(1, (await scenario.GetUserAsync(user.Id)).AuthorizationVersion);
	}

	[Fact]
	public async Task ChangeRequiresSameActiveAccountAndResetReleasesPendingAddress()
	{
		await using var scenario = environment.CreateScenario();
		var user = await scenario.RegisterAsync(true);
		var other = await scenario.RegisterAsync(true);
		var ownerSession = await scenario.CreateSessionAsync(user);
		var otherSession = await scenario.CreateSessionAsync(other);
		var newEmail = Guid.NewGuid() + "@example.test";
		var start = scenario.Job(EmailAction.StartEmailChange) with
		{
			UserId = user.Id,
			SessionId = ownerSession.Id,
			Email = newEmail,
			VerifiedAuthorizationVersion = user.AuthorizationVersion,
		};
		Assert.Equal("succeeded", (await scenario.ProcessAsync(start)).Status);
		await scenario.RecoverAsync(user.Id);
		var token = scenario.LastMessage(user.Id, "EmailChange");
		Assert.Equal(
			"failed",
			(
				await scenario.ProcessAsync(
					scenario.TokenJob(EmailAction.ConfirmEmailChange, token) with
					{
						UserId = other.Id,
						SessionId = otherSession.Id,
					}
				)
			).Status
		);
		await scenario.ProcessAsync(scenario.Job(EmailAction.RequestPasswordReset) with { Login = user.Email });
		await scenario.RecoverAsync(user.Id);
		Assert.Equal(
			"succeeded",
			(
				await scenario.ProcessAsync(
					scenario.TokenJob(
						EmailAction.ResetPassword,
						scenario.LastMessage(user.Id, "PasswordReset"),
						environment.Hasher.Hash("ResetPassword!")
					)
				)
			).Status
		);
		Assert.Null((await scenario.GetUserAsync(user.Id)).PendingEmail);
		Assert.Equal(
			"failed",
			(
				await scenario.ProcessAsync(
					scenario.TokenJob(EmailAction.ConfirmEmailChange, token) with
					{
						SessionId = ownerSession.Id,
					}
				)
			).Status
		);
		Assert.NotNull(await scenario.RegisterAsync(email: newEmail));
	}

	[Fact]
	public async Task RecoverySurvivesResetRequiresNewPasswordAndReservesOldEmail()
	{
		await using var scenario = environment.CreateScenario();
		var original = await scenario.RegisterAsync(true);
		var (changed, recovery) = await scenario.ChangeEmailAsync(original);
		Assert.Equal(original.Email, changed.PreviousEmail);
		Assert.Equal(changed.EmailRecoveryExpiresAt, recovery.ExpiresAt);
		Assert.Equal(1, changed.AuthorizationVersion);
		await scenario.ClearCooldownAsync(original.Email!, "registration");
		var blocked = scenario.Job(EmailAction.Register) with
		{
			Login = "blocked" + Guid.NewGuid().ToString("N"),
			Email = original.Email,
			PasswordHash = original.PasswordHash,
		};
		Assert.Equal("identity_unavailable", (await scenario.ProcessAsync(blocked)).Code);
		var currentSession = await scenario.CreateSessionAsync(changed);
		Assert.Equal(
			"email_change_unavailable",
			(
				await scenario.ProcessAsync(
					scenario.Job(EmailAction.StartEmailChange) with
					{
						UserId = changed.Id,
						SessionId = currentSession.Id,
						Email = Guid.NewGuid() + "@example.test",
						VerifiedAuthorizationVersion = changed.AuthorizationVersion,
					}
				)
			).Code
		);
		await scenario.ProcessAsync(scenario.Job(EmailAction.RequestPasswordReset) with { Login = changed.Email });
		await scenario.RecoverAsync(changed.Id);
		await scenario.ProcessAsync(
			scenario.TokenJob(
				EmailAction.ResetPassword,
				scenario.LastMessage(changed.Id, "PasswordReset"),
				environment.Hasher.Hash("ResetPassword!")
			)
		);
		Assert.NotNull(
			await scenario.Queue.FindTokenAsync(EmailIdentity.Fingerprint(recovery.Token!), CancellationToken.None)
		);
		var hash = environment.Hasher.Hash("RecoveryPassword!");
		var recover = scenario.TokenJob(EmailAction.RecoverEmail, recovery, hash);
		Assert.Equal("succeeded", (await scenario.ProcessAsync(recover)).Status);
		var restored = await scenario.GetUserAsync(original.Id);
		Assert.Equal(original.Email, restored.Email);
		Assert.Equal(hash, restored.PasswordHash);
		Assert.Equal(3, restored.AuthorizationVersion);
		Assert.Null(restored.PreviousEmail);
		Assert.Null(restored.CompletedEmailChangeId);
		Assert.Equal("failed", (await scenario.ProcessAsync(recover with { Id = Guid.NewGuid() })).Status);
		Assert.Equal("succeeded", (await scenario.ProcessAsync(recover)).Status);
		await scenario.RecoverAsync(original.Id);
		Assert.Contains(
			scenario.Messages,
			message => message.Recipient == changed.Email && message.Kind == "email_replaced"
		);
		Assert.Contains(
			scenario.Messages,
			message => message.Recipient == original.Email && message.Kind == "email_recovered"
		);
	}

	[Fact]
	public async Task CrossColumnUniquenessIsEnforcedByMySqlUnderConcurrency()
	{
		await using var scenario = environment.CreateScenario();
		var user = await scenario.RegisterAsync(true);
		var session = await scenario.CreateSessionAsync(user);
		var contested = Guid.NewGuid() + "@example.test";
		var registration = scenario.Job(EmailAction.Register) with
		{
			Login = "race" + Guid.NewGuid().ToString("N"),
			Email = contested,
			PasswordHash = user.PasswordHash,
		};
		var change = scenario.Job(EmailAction.StartEmailChange) with
		{
			UserId = user.Id,
			SessionId = session.Id,
			Email = contested,
			VerifiedAuthorizationVersion = user.AuthorizationVersion,
		};
		var results = await Task.WhenAll(scenario.ProcessAsync(registration), scenario.ProcessAsync(change));
		Assert.Single(results, result => result.Status == "succeeded");
		Assert.Single(results, result => result.Code == "identity_unavailable");
		await using var db = environment.CreateDb();
		var normalized = EmailIdentity.NormalizeEmail(contested);
		Assert.Equal(
			1,
			await db.Users.CountAsync(user =>
				user.NormalizedEmail == normalized || user.NormalizedPendingEmail == normalized
			)
		);
	}

	[Fact]
	public async Task DatabaseFailureKeepsTokenAndRetryCommitsOnce()
	{
		await using var scenario = environment.CreateScenario();
		var user = await scenario.RegisterAsync(true);
		await scenario.ProcessAsync(scenario.Job(EmailAction.RequestPasswordReset) with { Login = user.Email });
		await scenario.RecoverAsync(user.Id);
		var message = scenario.LastMessage(user.Id, "PasswordReset");
		var job = scenario.TokenJob(EmailAction.ResetPassword, message, environment.Hasher.Hash("ResetPassword!"));
		await Assert.ThrowsAsync<IOException>(() =>
			scenario.ProcessAsync(job, null, new FailNextUserSaveInterceptor())
		);
		Assert.NotNull(await scenario.Queue.FindTokenAsync(job.TokenFingerprint!, CancellationToken.None));
		Assert.Equal(user.PasswordHash, (await scenario.GetUserAsync(user.Id)).PasswordHash);
		Assert.Equal("succeeded", (await scenario.ProcessAsync(job)).Status);
		Assert.Equal(1, (await scenario.GetUserAsync(user.Id)).AuthorizationVersion);
	}

	[Fact]
	public async Task FailureAfterCommitReconcilesResultAndDoesNotApplyAgain()
	{
		await using var scenario = environment.CreateScenario();
		var user = await scenario.RegisterAsync(true);
		await scenario.ProcessAsync(scenario.Job(EmailAction.RequestPasswordReset) with { Login = user.Login });
		await scenario.RecoverAsync(user.Id);
		var job = scenario.TokenJob(
			EmailAction.ResetPassword,
			scenario.LastMessage(user.Id, "PasswordReset"),
			environment.Hasher.Hash("ResetPassword!")
		);
		var accepted = await scenario.Queue.PublishAsync(job, CancellationToken.None);
		var fault = InterfaceDouble.Create<IEmailQueue>(
			(method, args) =>
			{
				if (method.Name == nameof(IEmailQueue.FinishTokenAsync))
					return Task.FromException(new IOException("Injected Redis failure after MySQL commit."));
				return method.Invoke(scenario.Queue, args);
			}
		);
		await Assert.ThrowsAsync<IOException>(() => scenario.ProcessAsync(job, fault));
		Assert.Equal(1, (await scenario.GetUserAsync(user.Id)).AuthorizationVersion);
		await scenario.RecoverAsync(user.Id);
		Assert.Equal(
			"succeeded",
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
		Assert.Equal("succeeded", (await scenario.ProcessAsync(job)).Status);
		Assert.Equal(1, (await scenario.GetUserAsync(user.Id)).AuthorizationVersion);
		Assert.Null(await scenario.Queue.FindTokenAsync(job.TokenFingerprint!, CancellationToken.None));
	}

	[Fact]
	public async Task LoginRaceWithResetCannotCreateUsableSession()
	{
		await using var scenario = environment.CreateScenario();
		var user = await scenario.RegisterAsync(true);
		await scenario.ProcessAsync(scenario.Job(EmailAction.RequestPasswordReset) with { Login = user.Email });
		await scenario.RecoverAsync(user.Id);
		var reset = scenario.TokenJob(
			EmailAction.ResetPassword,
			scenario.LastMessage(user.Id, "PasswordReset"),
			environment.Hasher.Hash("ResetPassword!")
		);
		async Task CreateAndResetAsync(SessionData session, string token, TimeSpan ttl)
		{
			await environment.Sessions.CreateSessionAsync(session, token, ttl);
			Assert.Equal("succeeded", (await scenario.ProcessAsync(reset)).Status);
		}
		var racingSessions = InterfaceDouble.Create<ISessionRepository>(
			(method, args) =>
			{
				if (method.Name == nameof(ISessionRepository.CreateSessionAsync))
					return CreateAndResetAsync((SessionData)args[0]!, (string)args[1]!, (TimeSpan)args[2]!);
				return method.Invoke(environment.Sessions, args);
			}
		);
		await using var db = environment.CreateDb();
		var users = new EfRepository<User>(db);
		var jwt = new JwtHelper(environment.JwtOptions);
		var handler = new LoginHandler(
			users,
			environment.Hasher,
			jwt,
			racingSessions,
			new AuthTokenFactory(jwt, environment.JwtOptions, environment.CookieOptions),
			new AccountSessionValidator(users)
		);
		await Assert.ThrowsAsync<InvalidCredentialsException>(() =>
			handler.Handle(new LoginCommand(user.Login, "OriginalPassword!", "test"), CancellationToken.None).AsTask()
		);
		Assert.Empty(await environment.Sessions.ListByUserAsync(user.Id));
	}

	[Fact]
	public async Task RefreshRaceWithResetRejectsRotatedSession()
	{
		await using var scenario = environment.CreateScenario();
		var user = await scenario.RegisterAsync(true);
		await using var loginDb = environment.CreateDb();
		var tokens = await scenario
			.LoginHandler(loginDb)
			.Handle(new LoginCommand(user.Login, "OriginalPassword!", "test"), CancellationToken.None);
		await scenario.ProcessAsync(scenario.Job(EmailAction.RequestPasswordReset) with { Login = user.Email });
		await scenario.RecoverAsync(user.Id);
		var reset = scenario.TokenJob(
			EmailAction.ResetPassword,
			scenario.LastMessage(user.Id, "PasswordReset"),
			environment.Hasher.Hash("ResetPassword!")
		);
		async Task<bool> RotateAndResetAsync(
			SessionData session,
			string oldToken,
			string newToken,
			TimeSpan ttl,
			TimeSpan usedTtl
		)
		{
			var rotated = await environment.Sessions.TryRotateRefreshAsync(session, oldToken, newToken, ttl, usedTtl);
			await scenario.ProcessAsync(reset);
			return rotated;
		}
		var racingSessions = InterfaceDouble.Create<ISessionRepository>(
			(method, args) =>
			{
				if (method.Name == nameof(ISessionRepository.TryRotateRefreshAsync))
					return RotateAndResetAsync(
						(SessionData)args[0]!,
						(string)args[1]!,
						(string)args[2]!,
						(TimeSpan)args[3]!,
						(TimeSpan)args[4]!
					);
				return method.Invoke(environment.Sessions, args);
			}
		);
		await using var db = environment.CreateDb();
		var users = new EfRepository<User>(db);
		var jwt = new JwtHelper(environment.JwtOptions);
		var handler = new RefreshTokensHandler(
			users,
			jwt,
			racingSessions,
			new AuthTokenFactory(jwt, environment.JwtOptions, environment.CookieOptions),
			new AccountSessionValidator(users)
		);
		await Assert.ThrowsAsync<InvalidTokenException>(() =>
			handler.Handle(new RefreshTokensCommand(tokens.RefreshToken), CancellationToken.None).AsTask()
		);
		Assert.Empty(await environment.Sessions.ListByUserAsync(user.Id));
	}

	[Fact]
	public async Task QueueDelayDoesNotExtendTokenDeadlineAndExpiredJobsDoNotApply()
	{
		await using var scenario = environment.CreateScenario();
		var created = EmailIdentity.UtcNow().AddMinutes(-10);
		var job = scenario.Job(EmailAction.Register) with
		{
			CreatedAt = created,
			Login = "delay" + Guid.NewGuid().ToString("N"),
			Email = Guid.NewGuid() + "@example.test",
			PasswordHash = environment.Hasher.Hash("OriginalPassword!"),
		};
		Assert.Equal("succeeded", (await scenario.ProcessAsync(job)).Status);
		await using var db = environment.CreateDb();
		var user = await db.Users.SingleAsync(user => user.Login == job.Login);
		Assert.Equal(created.AddHours(24), user.EmailConfirmationExpiresAt);
		var expired = job with
		{
			Id = Guid.NewGuid(),
			Login = "expired" + Guid.NewGuid().ToString("N"),
			Email = Guid.NewGuid() + "@example.test",
			ExpiresAt = EmailIdentity.UtcNow().AddSeconds(-1),
		};
		Assert.Equal("processing_expired", (await scenario.ProcessAsync(expired)).Code);
		Assert.False(await db.Users.AnyAsync(user => user.Login == expired.Login));
	}

	[Fact]
	public async Task CleanupAndConfirmationCannotLeaveConfirmedAccountDeleted()
	{
		await using var scenario = environment.CreateScenario();
		var user = await scenario.RegisterAsync();
		await using var setupDb = environment.CreateDb();
		await setupDb
			.Users.Where(account => account.Id == user.Id)
			.ExecuteUpdateAsync(setters =>
				setters.SetProperty(account => account.RegisteredAt, EmailIdentity.UtcNow().AddDays(-8))
			);
		var job = scenario.TokenJob(EmailAction.ConfirmRegistration, scenario.LastMessage(user.Id, "Registration"));
		async Task CleanupAsync()
		{
			await using var db = environment.CreateDb();
			await new EmailWorkflowService(
				new EmailAccountStore(db),
				scenario.Queue,
				environment.Sessions
			).CleanupAsync(user.Id, CancellationToken.None);
		}
		await Task.WhenAll(scenario.ProcessAsync(job), CleanupAsync());
		Assert.False(await setupDb.Users.AnyAsync(account => account.Id == user.Id));
		var confirmed = await scenario.RegisterAsync(true);
		await setupDb
			.Users.Where(account => account.Id == confirmed.Id)
			.ExecuteUpdateAsync(setters =>
				setters.SetProperty(account => account.RegisteredAt, EmailIdentity.UtcNow().AddDays(-8))
			);
		await using var cleanupDb = environment.CreateDb();
		await new EmailWorkflowService(
			new EmailAccountStore(cleanupDb),
			scenario.Queue,
			environment.Sessions
		).CleanupAsync(confirmed.Id, CancellationToken.None);
		Assert.True(await setupDb.Users.AnyAsync(account => account.Id == confirmed.Id));
	}

	[Theory]
	[InlineData("bad@login")]
	[InlineData("")]
	public async Task InvalidLoginIsRejected(string login)
	{
		var result = await new RegisterCommandValidator().ValidateAsync(
			new RegisterCommand(login, "user@example.test", "OriginalPassword!")
		);
		Assert.False(result.IsValid);
	}

	[Fact]
	public void NormalizationPreservesDotsAndProviderIndependentAddresses()
	{
		Assert.Equal("A.B+TAG@EXAMPLE.TEST", EmailIdentity.NormalizeEmail(" a.b+tag@Example.test "));
		Assert.NotEqual(
			EmailIdentity.NormalizeEmail("a.b@example.test"),
			EmailIdentity.NormalizeEmail("ab@example.test")
		);
		Assert.Throws<BadRequestException>(() => EmailIdentity.ValidatePassword(new string('я', 40)));
	}
}
