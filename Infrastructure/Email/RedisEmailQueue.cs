using System.Collections.Concurrent;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using ViaTrade.Application.Common.Exceptions;
using ViaTrade.Application.Email;
using ViaTrade.Configuration.Options;

namespace ViaTrade.Infrastructure.Email;

public sealed class RedisEmailQueue : IEmailQueue, IEmailStreamWorker
{
	private const string Group = "workers";
	private readonly IDatabase _database;
	private readonly IConnectionMultiplexer _connection;
	private readonly IDataProtector _protector;
	private readonly EmailSettings _settings;
	private readonly ConcurrentDictionary<string, RedisValue> _pendingCursors = new();
	private readonly ConcurrentDictionary<string, RedisValue> _claimCursors = new();

	public RedisEmailQueue(
		IConnectionMultiplexer connection,
		IDataProtectionProvider protection,
		IOptions<EmailSettings> options
	)
	{
		_settings = options.Value;
		_connection = connection;
		_database = connection.GetDatabase(_settings.RedisDatabase);
		_protector = protection.CreateProtector("ViaTrade.Email.Queue.v1");
	}

	public async Task<EmailAcceptedResult> PublishAsync(EmailJob job, CancellationToken ct)
	{
		ct.ThrowIfCancellationRequested();
		RequireSupportedRedis();
		var secret = EmailIdentity.NewSecret();
		var initial = new EmailOperationResult("pending", "processing", "Request accepted for processing.");
		if (job.IsPrivateRequest)
			initial = EmailWorkflowService.PrivateResult;
		const string script = """
			if redis.call('XLEN', KEYS[1]) >= tonumber(ARGV[8]) then return 0 end
			redis.call('HSET', KEYS[2], 'access', ARGV[1], 'owner', ARGV[2], 'deadline', ARGV[3], 'private', ARGV[4], 'result', ARGV[5])
			redis.call('PEXPIRE', KEYS[2], 86400000)
			redis.call('XADD', KEYS[1], '*', 'operation_id', ARGV[6], 'payload', ARGV[7])
			return 1
			""";
		try
		{
			var accepted = (int)
				await _database.ScriptEvaluateAsync(
					script,
					[Stream(false), ResultKey(job.Id)],
					[
						EmailIdentity.Fingerprint(secret),
						Owner(job),
						new DateTimeOffset(job.ExpiresAt).ToUnixTimeMilliseconds(),
						job.IsPrivateRequest.ToString(),
						JsonSerializer.Serialize(initial),
						job.Id.ToString("N"),
						Protect(job),
						_settings.MaxQueuedMessages,
					]
				);
			if (accepted == 0)
				throw new ServiceUnavailableException("The email queue is at capacity.", "email_queue_unavailable");
		}
		catch (RedisException exception)
		{
			throw new ServiceUnavailableException(
				"The email queue is unavailable.",
				"email_queue_unavailable",
				exception
			);
		}

		return new EmailAcceptedResult(job.Id, secret, job.ExpiresAt);
	}

	public async Task<EmailOperationResult?> FindResultAsync(
		int? userId,
		Guid operationId,
		string? sessionId,
		string accessKey,
		CancellationToken ct
	)
	{
		ct.ThrowIfCancellationRequested();
		var values = await _database.HashGetAsync(ResultKey(operationId), ["access", "owner", "deadline", "result"]);
		if (
			values[0].IsNullOrEmpty
			|| !CryptographicOperations.FixedTimeEquals(
				Encoding.ASCII.GetBytes(values[0].ToString()),
				Encoding.ASCII.GetBytes(EmailIdentity.Fingerprint(accessKey))
			)
		)
			return null;

		if (
			!values[1].IsNullOrEmpty
			&& (values[1].ToString() != userId?.ToString(CultureInfo.InvariantCulture) || sessionId == null)
		)
			return null;

		var result = JsonSerializer.Deserialize<EmailOperationResult>(values[3].ToString());
		if (result?.Status == "pending" && (long)values[2] <= DateTimeOffset.UtcNow.ToUnixTimeMilliseconds())
			return new EmailOperationResult(
				"indeterminate",
				"processing_unavailable",
				"Processing exceeded its deadline. Check the account state before repeating the operation."
			);
		return result;
	}

	public async Task<EmailToken?> FindTokenAsync(string fingerprint, CancellationToken ct)
	{
		ct.ThrowIfCancellationRequested();
		var value = await _database.StringGetAsync(Key("token:" + fingerprint));
		if (value.IsNullOrEmpty)
			return null;

		var token = Unprotect<EmailToken>(value.ToString());
		if (token.ExpiresAt <= DateTime.UtcNow)
			return null;
		return token;
	}

	public async Task<EmailOperationResult?> FindTerminalResultAsync(Guid operationId, CancellationToken ct)
	{
		ct.ThrowIfCancellationRequested();
		var values = await _database.HashGetAsync(ResultKey(operationId), ["private", "result"]);
		if (values[0] == "True" || values[1].IsNullOrEmpty)
			return null;
		var result = JsonSerializer.Deserialize<EmailOperationResult>(values[1].ToString());
		if (result?.Status is "succeeded" or "failed")
			return result;
		return null;
	}

	public Task FinishTokenAsync(string fingerprint, CancellationToken ct)
	{
		ct.ThrowIfCancellationRequested();
		return _database.KeyDeleteAsync(Key("token:" + fingerprint));
	}

	public async Task PublishMailAsync(int userId, EmailIntent intent, CancellationToken ct)
	{
		ct.ThrowIfCancellationRequested();
		var ttl = intent.ExpiresAt - DateTime.UtcNow;
		if (ttl.TotalMilliseconds < 1)
			return;

		string? secret = null;
		var tokenValue = "";
		var tokenKey = Key("unusedToken");
		if (intent.Token != null)
		{
			secret = EmailIdentity.NewSecret();
			tokenKey = Key("token:" + EmailIdentity.Fingerprint(secret));
			tokenValue = Protect(intent.Token with { UserId = userId });
		}
		var message = new EmailMessage(intent.Id, userId, intent.Recipient, intent.Kind, secret, intent.ExpiresAt);
		const string script = """
			if redis.call('EXISTS', KEYS[2]) == 1 then return 1 end
			if redis.call('XLEN', KEYS[1]) >= tonumber(ARGV[5]) then return 0 end
			redis.call('SET', KEYS[2], ARGV[1], 'PX', ARGV[2])
			if ARGV[3] ~= '' then redis.call('SET', KEYS[3], ARGV[3], 'PX', ARGV[2]) end
			redis.call('XADD', KEYS[1], '*', 'operation_id', ARGV[4], 'payload', ARGV[1])
			return 1
			""";
		var published = (int)
			await _database.ScriptEvaluateAsync(
				script,
				[Stream(true), Key("event:" + intent.Id.ToString("N")), tokenKey],
				[
					Protect(message),
					(long)ttl.TotalMilliseconds,
					tokenValue,
					intent.Id.ToString("N"),
					_settings.MaxQueuedMessages,
				]
			);
		if (published == 0)
			throw new IOException("The email transport queue is at capacity.");
	}

	public Task SetResultAsync(Guid operationId, EmailOperationResult result, CancellationToken ct)
	{
		ct.ThrowIfCancellationRequested();
		const string script = """
			if redis.call('EXISTS', KEYS[1]) == 0 then return 0 end
			if redis.call('HGET', KEYS[1], 'private') == 'True' then return 1 end
			local current = redis.call('HGET', KEYS[1], 'result')
			if current and cjson.decode(current).Status == 'succeeded' then return 1 end
			redis.call('HSET', KEYS[1], 'result', ARGV[1])
			return 1
			""";
		return _database.ScriptEvaluateAsync(script, [ResultKey(operationId)], [JsonSerializer.Serialize(result)]);
	}

	public async Task<string?> FindMailOutcomeAsync(Guid eventId, CancellationToken ct)
	{
		ct.ThrowIfCancellationRequested();
		var outcome = await _database.StringGetAsync(Key("mailOutcome:" + eventId.ToString("N")));
		if (outcome.IsNullOrEmpty)
			return null;
		return outcome.ToString();
	}

	public Task SetMailOutcomeAsync(EmailMessage message, bool accepted, CancellationToken ct)
	{
		ct.ThrowIfCancellationRequested();
		var ttl = message.ExpiresAt.AddHours(1) - DateTime.UtcNow;
		if (ttl <= TimeSpan.Zero)
			return Task.CompletedTask;

		var outcome = "failed";
		if (accepted)
			outcome = "acceptedByTransport";
		return _database.StringSetAsync(Key("mailOutcome:" + message.EventId.ToString("N")), outcome, ttl);
	}

	public async Task<bool> TryAcquireLimitAsync(
		string scope,
		string value,
		int limit,
		TimeSpan window,
		CancellationToken ct
	)
	{
		ct.ThrowIfCancellationRequested();
		const string script = """
			local count = redis.call('INCR', KEYS[1])
			if count == 1 then redis.call('PEXPIRE', KEYS[1], ARGV[1]) end
			return count
			""";
		return (long)
				await _database.ScriptEvaluateAsync(
					script,
					[Key("limit:" + scope + ":" + EmailIdentity.Fingerprint(value))],
					[(long)window.TotalMilliseconds]
				) <= limit;
	}

	public async Task<bool> TryAcquireMailCooldownAsync(
		string address,
		EmailAction action,
		Guid jobId,
		CancellationToken ct
	)
	{
		ct.ThrowIfCancellationRequested();
		var purpose = action switch
		{
			EmailAction.Register or EmailAction.ResendConfirmation => "registration",
			EmailAction.RequestPasswordReset => "passwordReset",
			EmailAction.StartEmailChange => "emailChange",
			_ => throw new InvalidOperationException("Unsupported mail cooldown."),
		};
		const string script = """
			local current = redis.call('GET', KEYS[1])
			if current == ARGV[1] then return 1 end
			if current then return 0 end
			redis.call('SET', KEYS[1], ARGV[1], 'PX', 300000)
			return 1
			""";
		return (int)
				await _database.ScriptEvaluateAsync(
					script,
					[Key("cooldown:" + purpose + ":" + EmailIdentity.Fingerprint(address))],
					[jobId.ToString("N")]
				) == 1;
	}

	public async Task InitializeAsync(CancellationToken ct)
	{
		ct.ThrowIfCancellationRequested();
		RequireSupportedRedis();

		foreach (var mail in new[] { false, true })
		{
			try
			{
				await _database.StreamCreateConsumerGroupAsync(Stream(mail), Group, "0-0", createStream: true);
			}
			catch (RedisServerException exception)
				when (exception.Message.StartsWith("BUSYGROUP", StringComparison.Ordinal)) { }
		}
	}

	public async Task<IReadOnlyList<EmailWorkItem>> ReadAsync(bool mail, string consumer, CancellationToken ct)
	{
		ct.ThrowIfCancellationRequested();
		var cursorKey = consumer + mail;
		var hasPendingCursor = _pendingCursors.TryGetValue(cursorKey, out var pendingCursor);
		if (!hasPendingCursor)
			pendingCursor = "0-0";
		var pending = await _database.StreamReadGroupAsync(
			Stream(mail),
			Group,
			consumer,
			pendingCursor,
			_settings.BatchSize
		);
		_pendingCursors[cursorKey] = "0-0";
		if (pending.Length > 0)
			_pendingCursors[cursorKey] = pending[^1].Id;

		var hasClaimCursor = _claimCursors.TryGetValue(cursorKey, out var claimCursor);
		if (!hasClaimCursor)
			claimCursor = "0-0";
		var claimed = await _database.StreamAutoClaimAsync(
			Stream(mail),
			Group,
			consumer,
			60000,
			claimCursor,
			_settings.BatchSize
		);
		_claimCursors[cursorKey] = claimed.NextStartId;
		var fresh = await _database.StreamReadGroupAsync(Stream(mail), Group, consumer, ">", _settings.BatchSize);
		List<EmailWorkItem> items = [];
		foreach (
			var entry in pending
				.Concat(claimed.ClaimedEntries)
				.Concat(fresh)
				.DistinctBy(entry => entry.Id)
				.Take(_settings.BatchSize)
		)
		{
			var meta = await _database.HashGetAsync(MetaKey(mail, entry.Id.ToString()), ["due", "attempt"]);
			if (!meta[0].IsNullOrEmpty && (long)meta[0] > DateTimeOffset.UtcNow.ToUnixTimeMilliseconds())
				continue;
			var attempt = 1;
			if (!meta[1].IsNullOrEmpty)
				attempt = (int)meta[1] + 1;
			try
			{
				var payload = entry.Values.First(value => value.Name == "payload").Value.ToString();
				if (mail)
					items.Add(new EmailWorkItem(entry.Id.ToString(), null, Unprotect<EmailMessage>(payload), attempt));
				else
					items.Add(new EmailWorkItem(entry.Id.ToString(), Unprotect<EmailJob>(payload), null, attempt));
			}
			catch (Exception exception)
				when (exception is CryptographicException or JsonException or InvalidOperationException)
			{
				await _database.StringSetAsync(
					Key("dead:" + mail + ":" + entry.Id),
					Protect(
						entry
							.Values.Select(value => new
							{
								Name = value.Name.ToString(),
								Value = value.Value.ToString(),
							})
							.ToArray()
					),
					TimeSpan.FromDays(7)
				);
				await _database.StreamAcknowledgeAsync(Stream(mail), Group, entry.Id);
				await _database.StreamDeleteAsync(Stream(mail), [entry.Id]);
			}
		}
		return items;
	}

	public async Task<int> TryLeaseAsync(bool mail, string streamId, string consumer, CancellationToken ct)
	{
		ct.ThrowIfCancellationRequested();
		const string script = """
			local due = redis.call('HGET', KEYS[2], 'due')
			if due and tonumber(due) > tonumber(ARGV[2]) then return 0 end
			if redis.call('SET', KEYS[1], ARGV[1], 'NX', 'PX', 120000) then
			 local attempt = redis.call('HINCRBY', KEYS[2], 'attempt', 1)
			 redis.call('PEXPIRE', KEYS[2], 259200000)
			 return attempt
			end
			return 0
			""";
		return (int)
			await _database.ScriptEvaluateAsync(
				script,
				[LeaseKey(mail, streamId), MetaKey(mail, streamId)],
				[consumer, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()]
			);
	}

	public Task CompleteAsync(bool mail, EmailWorkItem item, string consumer, CancellationToken ct)
	{
		ct.ThrowIfCancellationRequested();
		const string script = """
			if redis.call('GET', KEYS[2]) ~= ARGV[1] then return 0 end
			redis.call('XACK', KEYS[1], ARGV[2], ARGV[3])
			redis.call('XDEL', KEYS[1], ARGV[3])
			redis.call('DEL', KEYS[2], KEYS[3])
			return 1
			""";
		return _database.ScriptEvaluateAsync(
			script,
			[Stream(mail), LeaseKey(mail, item.StreamId), MetaKey(mail, item.StreamId)],
			[consumer, Group, item.StreamId]
		);
	}

	public async Task FailAsync(bool mail, EmailWorkItem item, string consumer, CancellationToken ct)
	{
		ct.ThrowIfCancellationRequested();
		if (item.Attempt >= _settings.MaxAttempts)
		{
			await _database.StringSetAsync(
				Key("dead:" + mail + ":" + item.StreamId),
				Protect(item),
				TimeSpan.FromDays(7)
			);
			await CompleteAsync(mail, item, consumer, ct);
			return;
		}

		var delay = Math.Min(_settings.RetryMaxSeconds, _settings.RetryBaseSeconds * Math.Pow(2, item.Attempt - 1));
		const string script = """
			if redis.call('GET', KEYS[1]) ~= ARGV[1] then return 0 end
			redis.call('HSET', KEYS[2], 'due', ARGV[2])
			redis.call('DEL', KEYS[1])
			return 1
			""";
		await _database.ScriptEvaluateAsync(
			script,
			[LeaseKey(mail, item.StreamId), MetaKey(mail, item.StreamId)],
			[consumer, DateTimeOffset.UtcNow.AddSeconds(delay).ToUnixTimeMilliseconds()]
		);
	}

	private string Protect<T>(T value) => _protector.Protect(JsonSerializer.Serialize(value));

	private void RequireSupportedRedis()
	{
		foreach (var endpoint in _connection.GetEndPoints())
		{
			var server = _connection.GetServer(endpoint);
			if (server.IsConnected && server.Version < new Version(6, 2))
				throw new NotSupportedException("Email queues require Redis 6.2 or newer for XAUTOCLAIM.");
		}
	}

	private T Unprotect<T>(string value) =>
		JsonSerializer.Deserialize<T>(_protector.Unprotect(value))
		?? throw new InvalidOperationException("Invalid protected email queue payload.");

	private RedisKey Key(string suffix) => _settings.StreamPrefix + ":" + suffix;

	private RedisKey Stream(bool mail)
	{
		if (mail)
			return Key("mail");
		return Key("actions");
	}

	private RedisKey ResultKey(Guid operationId) => Key("result:" + operationId.ToString("N"));

	private RedisKey MetaKey(bool mail, string streamId) => Key("meta:" + mail + ":" + streamId);

	private RedisKey LeaseKey(bool mail, string streamId) => Key("lease:" + mail + ":" + streamId);

	private static string Owner(EmailJob job)
	{
		if (job.SessionId != null)
			return job.UserId!.Value.ToString(CultureInfo.InvariantCulture);
		return "";
	}
}
