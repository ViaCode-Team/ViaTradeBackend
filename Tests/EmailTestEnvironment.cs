using System.Collections.Concurrent;
using System.Text.Json;
using Ardalis.Specification.EntityFrameworkCore;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using MySqlConnector;
using StackExchange.Redis;
using ViaTrade.Api.Swagger;
using ViaTrade.Application.Auth.Common;
using ViaTrade.Application.Auth.Common.Abstractions;
using ViaTrade.Application.Auth.Login;
using ViaTrade.Application.Auth.RefreshTokens;
using ViaTrade.Application.Email;
using ViaTrade.Configuration.Options;
using ViaTrade.Domain.Entities;
using ViaTrade.Infrastructure.DataBase;
using ViaTrade.Infrastructure.DataBase.Repositories.Generic;
using ViaTrade.Infrastructure.Email;
using ViaTrade.Infrastructure.Redis.Repositories;
using ViaTrade.Infrastructure.Services;

namespace ViaTrade.Tests;

[CollectionDefinition("Email integration", DisableParallelization = true)]
public sealed class EmailIntegrationCollection : ICollectionFixture<EmailTestEnvironment>;

public sealed class EmailTestEnvironment : IAsyncLifetime
{
	private readonly string _databaseName = "vt_email_" + Guid.NewGuid().ToString("N");
	private bool _databaseCreated;
	private string _adminConnection = "";
	private readonly string _keyPath = Path.Combine(
		Path.GetTempPath(),
		"viatrade_email_tests_" + Guid.NewGuid().ToString("N")
	);
	public string ConnectionString { get; private set; } = "";
	public ConnectionMultiplexer Redis { get; private set; } = null!;
	public IDataProtectionProvider Protection { get; private set; } = null!;
	public IConfiguration Configuration { get; private set; } = null!;
	public string MySqlVersion { get; private set; } = "";
	public string KeyPath => _keyPath;
	public string RedisVersion { get; private set; } = "";
	public IPasswordHasher Hasher { get; } = new BCryptPasswordHasher();
	public ISessionRepository Sessions { get; private set; } = null!;
	public IOptions<JwtSettings> JwtOptions { get; } =
		Options.Create(
			new JwtSettings
			{
				Secret = EmailIdentity.NewSecret(),
				Issuer = "ViaTradeTests",
				Audience = "ViaTradeTests",
				AccessTokenMinutes = 60,
			}
		);
	public IOptions<AuthCookieSettings> CookieOptions { get; } =
		Options.Create(
			new AuthCookieSettings
			{
				AccessTokenCookie = "access_token",
				RefreshTokenCookie = "refresh_token",
				RefreshTokenExpiryDays = 7,
				AbsoluteSessionLifetimeDays = 30,
			}
		);

	public async Task InitializeAsync()
	{
		var root = FindRoot();
		Configuration = new ConfigurationBuilder()
			.AddJsonFile(Path.Combine(root, "Api", "appsettings.json"))
			.AddJsonFile(Path.Combine(root, "Api", "appsettings.Development.json"))
			.AddUserSecrets(typeof(SwaggerHostFactory).Assembly, optional: true)
			.Build();
		var configuredMySql =
			Environment.GetEnvironmentVariable("VIATRADE_TEST_MYSQL") ?? Configuration.GetConnectionString("MySql")!;
		var builder = new MySqlConnectionStringBuilder(configuredMySql) { Database = "", Pooling = false };
		_adminConnection = builder.ConnectionString;
		await using var connection = new MySqlConnection(_adminConnection);
		await connection.OpenAsync();
		MySqlVersion = connection.ServerVersion;
		await using var command = connection.CreateCommand();
		command.CommandText = $"CREATE DATABASE `{_databaseName}` CHARACTER SET utf8mb4";
		await command.ExecuteNonQueryAsync();
		_databaseCreated = true;
		builder.Database = _databaseName;
		ConnectionString = builder.ConnectionString;
		await using var db = CreateDb();
		await db.Database.MigrateAsync();
		command.CommandText =
			$"ALTER TABLE `{_databaseName}`.`Users` AUTO_INCREMENT = {Random.Shared.Next(100000000, 200000000)}";
		await command.ExecuteNonQueryAsync();

		var redisOptions = ConfigurationOptions.Parse(
			Environment.GetEnvironmentVariable("VIATRADE_TEST_REDIS") ?? Configuration.GetConnectionString("Redis")!
		);
		redisOptions.DefaultDatabase = 15;
		Redis = await ConnectionMultiplexer.ConnectAsync(redisOptions);
		RedisVersion = Redis.GetServer(Redis.GetEndPoints()[0]).Version.ToString();
		Protection = DataProtectionProvider.Create(
			new DirectoryInfo(_keyPath),
			settings => settings.SetApplicationName("ViaTrade.Email.Tests")
		);
		Sessions = new SessionRedisRepository(Redis, NullLogger<SessionRedisRepository>.Instance);
	}

	public AppDbContext CreateDb(params IInterceptor[] interceptors)
	{
		var options = new DbContextOptionsBuilder<AppDbContext>()
			.UseMySql(
				ConnectionString,
				ServerVersion.AutoDetect(ConnectionString),
				settings => settings.EnableRetryOnFailure(2)
			)
			.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
			.AddInterceptors(interceptors)
			.Options;
		return new AppDbContext(options);
	}

	public EmailScenario CreateScenario() => new(this);

	public async Task DisposeAsync()
	{
		if (!_databaseCreated)
			return;
		if (Redis != null)
		{
			await using var db = CreateDb();
			foreach (var userId in await db.Users.Select(user => user.Id).ToListAsync())
			{
				foreach (var session in await Sessions.ListByUserAsync(userId))
					await Sessions.TerminateSessionAsync(session.Id);
			}
			await Redis.DisposeAsync();
		}

		await using var connection = new MySqlConnection(_adminConnection);
		await connection.OpenAsync();
		await using var command = connection.CreateCommand();
		command.CommandText = $"DROP DATABASE `{_databaseName}`";
		await command.ExecuteNonQueryAsync();
		if (Directory.Exists(_keyPath))
			Directory.Delete(_keyPath, true);
	}

	private static string FindRoot()
	{
		var directory = new DirectoryInfo(AppContext.BaseDirectory);
		while (directory != null && !File.Exists(Path.Combine(directory.FullName, "ViaTradeBackend.sln")))
			directory = directory.Parent;
		return directory?.FullName ?? throw new InvalidOperationException("Repository root not found.");
	}
}

public sealed class EmailScenario : IAsyncDisposable
{
	private readonly EmailTestEnvironment _environment;
	private readonly ConcurrentDictionary<int, bool> _userIds = new();
	public EmailSettings Settings { get; }
	public RedisEmailQueue Queue { get; }
	public IDatabase Database { get; }
	public List<EmailMessage> Messages { get; } = [];

	public EmailScenario(EmailTestEnvironment environment)
	{
		_environment = environment;
		Settings = new EmailSettings
		{
			RedisDatabase = 15,
			StreamPrefix = "viatrade_email_test:" + Guid.NewGuid().ToString("N"),
			RetryBaseSeconds = 1,
			RetryMaxSeconds = 1,
			MaxAttempts = 3,
			IpRequestsPerMinute = 1000,
			TokenAttemptsPerMinute = 100,
		};
		Queue = new RedisEmailQueue(environment.Redis, environment.Protection, Options.Create(Settings));
		Database = environment.Redis.GetDatabase(15);
	}

	public EmailJob Job(EmailAction action) =>
		new()
		{
			Id = Guid.NewGuid(),
			Action = action,
			CreatedAt = EmailIdentity.UtcNow(),
			ExpiresAt = EmailIdentity.UtcNow().AddMinutes(30),
		};

	public async Task<EmailOperationResult> ProcessAsync(
		EmailJob job,
		IEmailQueue? queue = null,
		params IInterceptor[] interceptors
	)
	{
		await using var db = _environment.CreateDb(interceptors);
		var result = await new EmailWorkflowService(
			new EmailAccountStore(db),
			queue ?? Queue,
			_environment.Sessions
		).ProcessAsync(job, CancellationToken.None);
		if (job.Action == EmailAction.Register && result.Status == "succeeded")
		{
			var normalized = EmailIdentity.NormalizeLogin(job.Login!);
			var userId = await db
				.Users.Where(user => user.NormalizedLogin == normalized)
				.Select(user => user.Id)
				.SingleAsync();
			_userIds.TryAdd(userId, true);
		}
		return result;
	}

	public async Task RecoverAsync(int userId)
	{
		await using var db = _environment.CreateDb();
		await new EmailWorkflowService(new EmailAccountStore(db), Queue, _environment.Sessions).RecoverAsync(
			userId,
			CancellationToken.None
		);
		await Queue.InitializeAsync(CancellationToken.None);
		for (var index = 0; index < 10; index++)
		{
			var items = await Queue.ReadAsync(true, "test-mail", CancellationToken.None);
			if (items.Count == 0)
				break;
			foreach (var item in items)
			{
				Messages.Add(item.Message!);
				Assert.True(await Queue.TryLeaseAsync(true, item.StreamId, "test-mail", CancellationToken.None) > 0);
				await Queue.CompleteAsync(true, item, "test-mail", CancellationToken.None);
			}
		}
	}

	public async Task<User> RegisterAsync(bool confirmed = false, string? email = null)
	{
		var suffix = Guid.NewGuid().ToString("N");
		var job = Job(EmailAction.Register) with
		{
			Login = "user" + suffix,
			Email = email ?? suffix + "@example.test",
			PasswordHash = _environment.Hasher.Hash("OriginalPassword!"),
		};
		Assert.Equal("succeeded", (await ProcessAsync(job)).Status);
		await using var db = _environment.CreateDb();
		var user = await db.Users.SingleAsync(user => user.NormalizedLogin == EmailIdentity.NormalizeLogin(job.Login));
		await RecoverAsync(user.Id);
		if (confirmed)
			Assert.Equal(
				"succeeded",
				(
					await ProcessAsync(TokenJob(EmailAction.ConfirmRegistration, LastMessage(user.Id, "Registration")))
				).Status
			);
		return await GetUserAsync(user.Id);
	}

	public EmailMessage LastMessage(int userId, string kind) =>
		Messages.Last(message => message.UserId == userId && message.Kind == kind);

	public EmailJob TokenJob(EmailAction action, EmailMessage message, string? password = null) =>
		Job(action) with
		{
			UserId = message.UserId,
			TokenFingerprint = EmailIdentity.Fingerprint(message.Token!),
			PasswordHash = password,
		};

	public async Task<User> GetUserAsync(int userId)
	{
		_userIds.TryAdd(userId, true);
		await using var db = _environment.CreateDb();
		return await db.Users.SingleAsync(user => user.Id == userId);
	}

	public async Task<SessionData> CreateSessionAsync(User user)
	{
		var session = new SessionData
		{
			Id = Guid.NewGuid().ToString(),
			UserId = user.Id,
			AuthorizationVersion = user.AuthorizationVersion,
			UserAgent = "email-integration",
			CreatedAt = DateTime.UtcNow,
			ExpiresAt = DateTime.UtcNow.AddHours(1),
		};
		await _environment.Sessions.CreateSessionAsync(session, EmailIdentity.NewSecret(), TimeSpan.FromHours(1));
		return session;
	}

	public async Task<(User User, EmailMessage Recovery)> ChangeEmailAsync(User user, string? newEmail = null)
	{
		var session = await CreateSessionAsync(user);
		var start = Job(EmailAction.StartEmailChange) with
		{
			UserId = user.Id,
			SessionId = session.Id,
			VerifiedAuthorizationVersion = user.AuthorizationVersion,
			Email = newEmail ?? Guid.NewGuid().ToString("N") + "@example.test",
		};
		Assert.Equal("succeeded", (await ProcessAsync(start)).Status);
		await RecoverAsync(user.Id);
		var confirm = TokenJob(EmailAction.ConfirmEmailChange, LastMessage(user.Id, "EmailChange")) with
		{
			SessionId = session.Id,
		};
		Assert.Equal("succeeded", (await ProcessAsync(confirm)).Status);
		await RecoverAsync(user.Id);
		return (await GetUserAsync(user.Id), LastMessage(user.Id, "EmailRecovery"));
	}

	public async Task ClearCooldownAsync(string email, string purpose)
	{
		await Database.KeyDeleteAsync(
			Settings.StreamPrefix
				+ ":cooldown:"
				+ purpose
				+ ":"
				+ EmailIdentity.Fingerprint(EmailIdentity.NormalizeEmail(email))
		);
	}

	public async Task<string> StoreTokenAsync(EmailToken token)
	{
		var secret = EmailIdentity.NewSecret();
		var protector = _environment.Protection.CreateProtector("ViaTrade.Email.Queue.v1");
		await Database.StringSetAsync(
			Settings.StreamPrefix + ":token:" + EmailIdentity.Fingerprint(secret),
			protector.Protect(JsonSerializer.Serialize(token)),
			TimeSpan.FromHours(1)
		);
		return secret;
	}

	public LoginHandler LoginHandler(AppDbContext db)
	{
		var users = new EfRepository<User>(db, SpecificationEvaluator.Default);
		var jwt = new JwtHelper(_environment.JwtOptions);
		return new LoginHandler(
			users,
			_environment.Hasher,
			jwt,
			_environment.Sessions,
			new AuthTokenFactory(jwt, _environment.JwtOptions, _environment.CookieOptions),
			new AccountSessionValidator(users)
		);
	}

	public RefreshTokensHandler RefreshHandler(AppDbContext db)
	{
		var users = new EfRepository<User>(db, SpecificationEvaluator.Default);
		var jwt = new JwtHelper(_environment.JwtOptions);
		return new RefreshTokensHandler(
			users,
			jwt,
			_environment.Sessions,
			new AuthTokenFactory(jwt, _environment.JwtOptions, _environment.CookieOptions),
			new AccountSessionValidator(users)
		);
	}

	public async ValueTask DisposeAsync()
	{
		await using var db = _environment.CreateDb();
		foreach (var userId in _userIds.Keys)
		{
			foreach (var session in await _environment.Sessions.ListByUserAsync(userId))
				await _environment.Sessions.TerminateSessionAsync(session.Id);
			await db.Users.Where(user => user.Id == userId).ExecuteDeleteAsync();
		}
		var server = _environment.Redis.GetServer(_environment.Redis.GetEndPoints()[0]);
		await foreach (var key in server.KeysAsync(15, Settings.StreamPrefix + ":*"))
			await Database.KeyDeleteAsync(key);
	}
}

public sealed class FailNextUserSaveInterceptor : SaveChangesInterceptor
{
	public bool Enabled { get; set; } = true;

	public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
		DbContextEventData eventData,
		InterceptionResult<int> result,
		CancellationToken cancellationToken = default
	)
	{
		if (
			Enabled
			&& eventData
				.Context!.ChangeTracker.Entries<User>()
				.Any(entry => entry.State is EntityState.Added or EntityState.Modified)
		)
		{
			Enabled = false;
			throw new IOException("Injected database failure before commit.");
		}
		return ValueTask.FromResult(result);
	}
}
