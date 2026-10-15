using System.Text.Json;
using ViaTrade.Application.Auth.Common;
using ViaTrade.Application.Auth.Common.Abstractions;
using ViaTrade.Application.Common.Exceptions;
using ViaTrade.Domain.Entities;

namespace ViaTrade.Application.Email;

public sealed class EmailWorkflowService(IEmailAccountStore store, IEmailQueue queue, ISessionRepository sessions)
{
	public static EmailOperationResult PrivateResult { get; } =
		new("completed", "request_received", "If the account is eligible, an email will be sent.");

	public async Task<EmailOperationResult> ProcessAsync(EmailJob job, CancellationToken ct)
	{
		var previous = await FindProcessedAsync(job, ct);
		if (previous != null)
		{
			if (job.TokenFingerprint != null && previous.Status == "succeeded")
				await queue.FinishTokenAsync(job.TokenFingerprint, ct);
			return previous;
		}

		EmailToken? token = null;
		if (job.TokenFingerprint != null)
			token = await queue.FindTokenAsync(job.TokenFingerprint, ct);

		SessionData? session = null;
		if (job.SessionId != null)
			session = await sessions.FindByIdAsync(job.SessionId);

		var sendAllowed = true;
		if (job.Action is EmailAction.ResendConfirmation or EmailAction.RequestPasswordReset)
		{
			var address = await store.ExecuteAsync(
				async transaction =>
				{
					var account = await transaction.FindByIdentityAsync(job.Login!, ct);
					return account?.NormalizedEmail;
				},
				ct
			);
			if (address != null)
				sendAllowed = await queue.TryAcquireMailCooldownAsync(address, job.Action, job.Id, ct);
		}
		else if (job.Action is EmailAction.Register or EmailAction.StartEmailChange)
			sendAllowed = await queue.TryAcquireMailCooldownAsync(
				EmailIdentity.NormalizeEmail(job.Email!),
				job.Action,
				job.Id,
				ct
			);

		EmailOperationResult result;
		try
		{
			result = await store.ExecuteAsync(
				async transaction =>
				{
					User? user;
					if (job.UserId is int userId)
						user = await transaction.FindByUserIdAsync(userId, ct);
					else if (job.TokenFingerprint != null)
						user = null;
					else
						user = await transaction.FindByIdentityAsync(job.Login!, ct);

					var now = EmailIdentity.UtcNow();
					var work = ReadWork(user);
					var receipt = work.Receipts.Find(receipt => receipt.Id == job.Id);
					if (receipt != null)
						return receipt.Result;

					if (now >= job.ExpiresAt)
						throw new BusinessRuleException("The processing deadline has expired.", "processing_expired");

					if (user != null)
					{
						work.Receipts.RemoveAll(receipt => receipt.RetainUntil <= now);
						work.Intents.RemoveAll(intent =>
							intent.ExpiresAt <= now || intent.AcceptedByTransportAt != null || intent.PermanentlyFailed
						);
						var issuesMail =
							job.Action
							is EmailAction.ResendConfirmation
								or EmailAction.RequestPasswordReset
								or EmailAction.StartEmailChange;
						if (work.Receipts.Count >= 128 || issuesMail && work.Intents.Count > 28)
							throw new BusinessRuleException("Too many pending account operations.", "account_busy");
					}

					if (job.SessionId != null)
						ValidateSession(user, session);

					var outcome = await ApplyAsync(transaction, user, work, job, token, sendAllowed, now, ct);
					user = outcome.User;
					if (user != null)
					{
						work.Receipts.Add(new EmailReceipt(job.Id, job.ExpiresAt.AddHours(1), outcome.Result));
						user.EmailWorkStateJson = JsonSerializer.Serialize(work);
						transaction.Update(user);
					}
					return outcome.Result;
				},
				ct
			);
		}
		catch (AppException exception)
		{
			result = new EmailOperationResult("failed", exception.Code, exception.Message);
		}

		if (job.IsPrivateRequest)
			return PrivateResult;

		if (job.TokenFingerprint != null && result.Status == "succeeded")
			await queue.FinishTokenAsync(job.TokenFingerprint, ct);

		return result;
	}

	public Task<EmailOperationResult?> FindProcessedAsync(EmailJob job, CancellationToken ct)
	{
		return store.ExecuteAsync(
			async transaction =>
			{
				User? user = null;
				if (job.UserId is int userId)
					user = await transaction.FindByUserIdAsync(userId, ct);
				else if (job.Login != null)
					user = await transaction.FindByIdentityAsync(job.Login, ct);
				return ReadWork(user).Receipts.Find(receipt => receipt.Id == job.Id)?.Result;
			},
			ct
		);
	}

	public async Task RecoverOperationAsync(EmailJob job, CancellationToken ct)
	{
		var userId = await store.ExecuteAsync(
			async transaction =>
			{
				User? user = null;
				if (job.UserId is int ownerId)
					user = await transaction.FindByUserIdAsync(ownerId, ct);
				else if (job.Login != null)
					user = await transaction.FindByIdentityAsync(job.Login, ct);
				return user?.Id;
			},
			ct
		);
		if (userId != null)
			await RecoverAsync(userId.Value, ct);
	}

	public async Task RecoverAsync(int userId, CancellationToken ct)
	{
		var snapshot = await store.ExecuteAsync(
			async transaction =>
			{
				var user = await transaction.FindByUserIdAsync(userId, ct);
				return ReadWork(user);
			},
			ct
		);

		foreach (var receipt in snapshot.Receipts)
			await queue.SetResultAsync(receipt.Id, receipt.Result, ct);

		foreach (
			var intent in snapshot.Intents.Where(intent =>
				intent.AcceptedByTransportAt == null && !intent.PermanentlyFailed && intent.ExpiresAt > DateTime.UtcNow
			)
		)
		{
			var outcome = await queue.FindMailOutcomeAsync(intent.Id, ct);
			if (outcome == "acceptedByTransport")
				await MarkMailAsync(userId, intent.Id, true, ct);
			else if (outcome == "failed")
				await MarkMailAsync(userId, intent.Id, false, ct);
			else
				await queue.PublishMailAsync(userId, intent, ct);
		}

		if (snapshot.RevokeSessionsBeforeVersion is long version)
		{
			var page = await sessions.ExecuteTerminateStalePageAsync(
				userId,
				version,
				snapshot.SessionRevocationOffset,
				100
			);

			await store.ExecuteAsync(
				async transaction =>
				{
					var user = await transaction.FindByUserIdAsync(userId, ct);
					if (user == null)
						return false;
					var work = ReadWork(user);
					if (work.RevokeSessionsBeforeVersion == version)
					{
						work.SessionRevocationOffset = page.NextOffset;
						if (page.Complete)
						{
							work.RevokeSessionsBeforeVersion = null;
							work.SessionRevocationOffset = 0;
						}
						user.EmailWorkStateJson = JsonSerializer.Serialize(work);
						transaction.Update(user);
					}
					return true;
				},
				ct
			);
		}
	}

	public Task MarkMailAsync(int userId, Guid eventId, bool accepted, CancellationToken ct)
	{
		return store.ExecuteAsync(
			async transaction =>
			{
				var user = await transaction.FindByUserIdAsync(userId, ct);
				if (user == null)
					return false;

				var work = ReadWork(user);
				var intent = work.Intents.Find(intent => intent.Id == eventId);
				if (intent != null)
				{
					if (accepted)
						intent.AcceptedByTransportAt = DateTime.UtcNow;
					else
						intent.PermanentlyFailed = true;
					user.EmailWorkStateJson = JsonSerializer.Serialize(work);
					transaction.Update(user);
				}
				return true;
			},
			ct
		);
	}

	public Task<bool> CanSendMailAsync(EmailMessage message, CancellationToken ct)
	{
		return store.ExecuteAsync(
			async transaction =>
			{
				var user = await transaction.FindByUserIdAsync(message.UserId, ct);
				if (user == null || message.ExpiresAt <= DateTime.UtcNow)
					return false;

				var intent = ReadWork(user).Intents.Find(intent => intent.Id == message.EventId);
				if (intent == null || intent.AcceptedByTransportAt != null || intent.PermanentlyFailed)
					return false;

				return intent.Token == null || IsCurrentToken(user, intent.Token, DateTime.UtcNow);
			},
			ct
		);
	}

	public Task CleanupAsync(int userId, CancellationToken ct)
	{
		return store.ExecuteAsync(
			async transaction =>
			{
				var user = await transaction.FindByUserIdAsync(userId, ct);
				if (user == null)
					return false;

				var now = DateTime.UtcNow;
				if (user.Email != null && user.EmailConfirmedAt == null && user.RegisteredAt <= now.AddDays(-7))
				{
					transaction.Delete(user);
					return true;
				}

				if (user.EmailChangeExpiresAt <= now)
					ClearPendingEmail(user);
				if (user.EmailRecoveryExpiresAt <= now)
					ClearPreviousEmail(user);
				if (user.PasswordResetExpiresAt <= now)
				{
					user.PasswordResetOperationId = null;
					user.PasswordResetExpiresAt = null;
				}

				var work = ReadWork(user);
				work.Receipts.RemoveAll(receipt => receipt.RetainUntil <= now);
				work.Intents.RemoveAll(intent =>
					intent.ExpiresAt <= now || intent.AcceptedByTransportAt != null || intent.PermanentlyFailed
				);
				user.EmailWorkStateJson = null;
				if (work.Receipts.Count > 0 || work.Intents.Count > 0 || work.RevokeSessionsBeforeVersion != null)
					user.EmailWorkStateJson = JsonSerializer.Serialize(work);
				transaction.Update(user);
				return true;
			},
			ct
		);
	}

	private static async Task<(User? User, EmailOperationResult Result)> ApplyAsync(
		IEmailAccountTransaction transaction,
		User? user,
		EmailWorkState work,
		EmailJob job,
		EmailToken? token,
		bool sendAllowed,
		DateTime now,
		CancellationToken ct
	)
	{
		if (job.Action == EmailAction.Register)
		{
			if (user != null)
				throw new ConflictException("Login or email is unavailable.", "identity_unavailable");
			if (!sendAllowed)
				throw new BusinessRuleException("Please wait before requesting another email.", "email_cooldown");
			await RequireAvailableEmailAsync(transaction, job.Email!, now, ct);
			user = new User
			{
				Login = job.Login!,
				NormalizedLogin = EmailIdentity.NormalizeLogin(job.Login!),
				Email = job.Email!,
				NormalizedEmail = EmailIdentity.NormalizeEmail(job.Email!),
				PasswordHash = job.PasswordHash!,
				RegisteredAt = job.CreatedAt,
				EmailConfirmationOperationId = job.Id,
				EmailConfirmationExpiresAt = job.CreatedAt.AddHours(24),
			};
			await transaction.AddAsync(user, ct);
			AddTokenIntent(
				work,
				job,
				user,
				EmailTokenPurpose.Registration,
				user.Email,
				job.Id,
				user.EmailConfirmationExpiresAt.Value
			);
			return (user, Success("registration_pending", "Confirm your email before signing in."));
		}

		if (job.IsPrivateRequest)
		{
			if (user?.Email == null || !sendAllowed)
				return (null, PrivateResult);

			if (job.Action == EmailAction.ResendConfirmation)
			{
				if (user.EmailConfirmedAt != null || user.RegisteredAt <= now.AddDays(-7))
					return (null, PrivateResult);
				if (user.EmailConfirmationExpiresAt == null || user.EmailConfirmationExpiresAt <= now)
				{
					user.EmailConfirmationOperationId = job.Id;
					user.EmailConfirmationExpiresAt = job.CreatedAt.AddHours(24);
				}
				AddTokenIntent(
					work,
					job,
					user,
					EmailTokenPurpose.Registration,
					user.Email,
					user.EmailConfirmationOperationId!.Value,
					user.EmailConfirmationExpiresAt!.Value
				);
			}
			else
			{
				if (user.PasswordResetExpiresAt == null || user.PasswordResetExpiresAt <= now)
				{
					user.PasswordResetOperationId = job.Id;
					user.PasswordResetExpiresAt = job.CreatedAt.AddMinutes(30);
				}
				AddTokenIntent(
					work,
					job,
					user,
					EmailTokenPurpose.PasswordReset,
					user.Email,
					user.PasswordResetOperationId!.Value,
					user.PasswordResetExpiresAt!.Value
				);
			}
			return (user, PrivateResult);
		}

		if (user == null)
			throw new InvalidTokenException();

		if (job.Action == EmailAction.StartEmailChange)
		{
			if (user.EmailConfirmedAt == null || user.AuthorizationVersion != job.VerifiedAuthorizationVersion)
				throw new AuthenticationException();
			if (user.EmailRecoveryExpiresAt > now)
				throw new BusinessRuleException(
					"An email change or recovery period is already active.",
					"email_change_unavailable"
				);
			if (!sendAllowed)
				throw new BusinessRuleException("Please wait before requesting another email.", "email_cooldown");
			if (user.EmailChangeExpiresAt > now)
			{
				if (user.NormalizedPendingEmail != EmailIdentity.NormalizeEmail(job.Email!))
					throw new BusinessRuleException("An email change is already active.", "email_change_unavailable");
				AddTokenIntent(
					work,
					job,
					user,
					EmailTokenPurpose.EmailChange,
					user.PendingEmail!,
					user.EmailChangeOperationId!.Value,
					user.EmailChangeExpiresAt!.Value
				);
				return (
					user,
					Success("email_change_pending", "Confirm the new email while signed in to this account.")
				);
			}
			await RequireAvailableEmailAsync(transaction, job.Email!, now, ct);
			user.PendingEmail = job.Email;
			user.NormalizedPendingEmail = EmailIdentity.NormalizeEmail(job.Email!);
			user.EmailChangeOperationId = job.Id;
			user.EmailChangeExpiresAt = job.CreatedAt.AddHours(24);
			AddTokenIntent(
				work,
				job,
				user,
				EmailTokenPurpose.EmailChange,
				user.PendingEmail!,
				job.Id,
				user.EmailChangeExpiresAt.Value
			);
			return (user, Success("email_change_pending", "Confirm the new email while signed in to this account."));
		}

		var purpose = job.Action switch
		{
			EmailAction.ConfirmRegistration => EmailTokenPurpose.Registration,
			EmailAction.ResetPassword => EmailTokenPurpose.PasswordReset,
			EmailAction.ConfirmEmailChange => EmailTokenPurpose.EmailChange,
			EmailAction.RecoverEmail => EmailTokenPurpose.EmailRecovery,
			_ => throw new InvalidOperationException("Unsupported email action."),
		};
		if (token == null || token.Purpose != purpose || token.UserId != user.Id || !IsCurrentToken(user, token, now))
			throw new InvalidTokenException();

		switch (job.Action)
		{
			case EmailAction.ConfirmRegistration:
				work.Intents.RemoveAll(intent => intent.Token?.Purpose == EmailTokenPurpose.Registration);
				user.EmailConfirmedAt = now;
				user.EmailConfirmationOperationId = null;
				user.EmailConfirmationExpiresAt = null;
				return (user, Success("email_confirmed", "Email confirmed. You can now sign in."));
			case EmailAction.ResetPassword:
				work.Intents.RemoveAll(intent =>
					intent.Token != null && intent.Token.Purpose != EmailTokenPurpose.EmailRecovery
				);
				user.PasswordHash = job.PasswordHash!;
				user.PasswordResetOperationId = null;
				user.PasswordResetExpiresAt = null;
				ClearPendingEmail(user);
				RevokeSessions(user, work);
				AddNotice(work, job, "password_reset", user.Email!, now);
				if (user.EmailConfirmedAt == null)
				{
					user.EmailConfirmationOperationId = job.Id;
					user.EmailConfirmationExpiresAt = now.AddHours(24);
					AddTokenIntent(
						work,
						job,
						user,
						EmailTokenPurpose.Registration,
						user.Email!,
						job.Id,
						user.EmailConfirmationExpiresAt.Value
					);
				}
				var resetMessage = "Password changed. Sign in again.";
				if (user.EmailConfirmedAt == null)
					resetMessage = "Password changed. Confirm your email with the new link before signing in.";
				return (user, Success("password_reset", resetMessage, true));
			case EmailAction.ConfirmEmailChange:
				work.Intents.RemoveAll(intent =>
					intent.Token != null && intent.Token.Purpose != EmailTokenPurpose.EmailRecovery
				);
				user.PreviousEmail = user.Email;
				user.NormalizedPreviousEmail = user.NormalizedEmail;
				user.CompletedEmailChangeId = user.EmailChangeOperationId;
				user.EmailRecoveryExpiresAt = now.AddHours(48);
				user.Email = user.PendingEmail;
				user.NormalizedEmail = user.NormalizedPendingEmail;
				user.EmailConfirmedAt = now;
				ClearPendingEmail(user);
				RevokeSessions(user, work);
				AddTokenIntent(
					work,
					job,
					user,
					EmailTokenPurpose.EmailRecovery,
					user.PreviousEmail!,
					user.CompletedEmailChangeId!.Value,
					user.EmailRecoveryExpiresAt.Value
				);
				AddNotice(work, job, "email_changed", user.Email!, now);
				return (user, Success("email_changed", "Email changed. Sign in again.", true));
			case EmailAction.RecoverEmail:
				work.Intents.RemoveAll(intent => intent.Token != null);
				var replacedEmail = user.Email!;
				user.Email = user.PreviousEmail;
				user.NormalizedEmail = user.NormalizedPreviousEmail;
				user.EmailConfirmedAt = now;
				user.PasswordHash = job.PasswordHash!;
				ClearPreviousEmail(user);
				ClearPendingEmail(user);
				user.PasswordResetOperationId = null;
				user.PasswordResetExpiresAt = null;
				RevokeSessions(user, work);
				AddNotice(work, job, "email_recovered", user.Email!, now);
				AddNotice(work, job, "email_replaced", replacedEmail, now);
				return (
					user,
					Success("email_recovered", "Previous email restored and password changed. Sign in again.", true)
				);
			default:
				throw new InvalidOperationException("Unsupported email action.");
		}
	}

	private static async Task RequireAvailableEmailAsync(
		IEmailAccountTransaction transaction,
		string email,
		DateTime now,
		CancellationToken ct
	)
	{
		if (await transaction.IsEmailOccupiedAsync(EmailIdentity.NormalizeEmail(email), now, ct))
			throw new ConflictException("Login or email is unavailable.", "identity_unavailable");
	}

	private static bool IsCurrentToken(User user, EmailToken token, DateTime now)
	{
		if (token.ExpiresAt <= now)
			return false;
		if (token.Purpose != EmailTokenPurpose.EmailRecovery && token.AuthorizationVersion != user.AuthorizationVersion)
			return false;

		return token.Purpose switch
		{
			EmailTokenPurpose.Registration => user.EmailConfirmedAt == null
				&& user.Email != null
				&& user.EmailConfirmationOperationId == token.OperationId
				&& user.EmailConfirmationExpiresAt == token.ExpiresAt
				&& user.RegisteredAt > now.AddDays(-7),
			EmailTokenPurpose.PasswordReset => user.PasswordResetOperationId == token.OperationId
				&& user.PasswordResetExpiresAt == token.ExpiresAt,
			EmailTokenPurpose.EmailChange => user.PendingEmail != null
				&& user.EmailChangeOperationId == token.OperationId
				&& user.EmailChangeExpiresAt == token.ExpiresAt
				&& (user.EmailRecoveryExpiresAt == null || user.EmailRecoveryExpiresAt <= now),
			EmailTokenPurpose.EmailRecovery => user.PreviousEmail != null
				&& user.CompletedEmailChangeId == token.OperationId
				&& user.EmailRecoveryExpiresAt == token.ExpiresAt,
			_ => false,
		};
	}

	private static void ValidateSession(User? user, SessionData? session)
	{
		if (
			user == null
			|| session == null
			|| session.UserId != user.Id
			|| session.AuthorizationVersion != user.AuthorizationVersion
			|| session.ExpiresAt <= DateTime.UtcNow
		)
			throw new AuthenticationException();
	}

	private static void RevokeSessions(User user, EmailWorkState work)
	{
		user.AuthorizationVersion++;
		work.RevokeSessionsBeforeVersion = user.AuthorizationVersion;
		work.SessionRevocationOffset = 0;
	}

	private static void ClearPendingEmail(User user)
	{
		user.PendingEmail = null;
		user.NormalizedPendingEmail = null;
		user.EmailChangeOperationId = null;
		user.EmailChangeExpiresAt = null;
	}

	private static void ClearPreviousEmail(User user)
	{
		user.PreviousEmail = null;
		user.NormalizedPreviousEmail = null;
		user.CompletedEmailChangeId = null;
		user.EmailRecoveryExpiresAt = null;
	}

	private static EmailWorkState ReadWork(User? user)
	{
		if (user?.EmailWorkStateJson == null)
			return new EmailWorkState();

		return JsonSerializer.Deserialize<EmailWorkState>(user.EmailWorkStateJson)
			?? throw new InvalidOperationException("Invalid email recovery state.");
	}

	private static void AddTokenIntent(
		EmailWorkState work,
		EmailJob job,
		User user,
		EmailTokenPurpose purpose,
		string recipient,
		Guid operationId,
		DateTime expiresAt
	)
	{
		work.Intents.Add(
			new EmailIntent
			{
				Id = EmailIdentity.EventId(job.Id, purpose.ToString()),
				Kind = purpose.ToString(),
				Recipient = recipient,
				ExpiresAt = expiresAt,
				Token = new EmailToken(purpose, user.Id, operationId, user.AuthorizationVersion, expiresAt),
			}
		);
	}

	private static void AddNotice(EmailWorkState work, EmailJob job, string kind, string recipient, DateTime now)
	{
		work.Intents.Add(
			new EmailIntent
			{
				Id = EmailIdentity.EventId(job.Id, kind),
				Kind = kind,
				Recipient = recipient,
				ExpiresAt = now.AddHours(48),
			}
		);
	}

	private static EmailOperationResult Success(string code, string message, bool clearCookies = false) =>
		new("succeeded", code, message, clearCookies);
}
