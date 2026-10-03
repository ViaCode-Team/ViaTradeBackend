using System.Reflection;
using StackExchange.Redis;
using ViaTrade.Application.Users.Models;
using ViaTrade.Infrastructure.Redis.Keys;
using ViaTrade.Infrastructure.Redis.Repositories;
using Xunit;

namespace ViaTrade.Tests;

public sealed class RedisCacheRepositoryTests
{
	[Fact]
	public async Task ConsumeUsesGetDeleteAndPreservesExistingTokenJson()
	{
		var database = DispatchProxy.Create<IDatabase, RedisDatabaseStub>();
		var stub = (RedisDatabaseStub)database;
		stub.Values["TgToken:test"] = """{"UserId":7,"Id":"test"}""";
		var repository = new BaseRedisRepository<TelegramTokenEntity>(database, RedisKeys.Cache.TelegramTokens);

		var token = await repository.ConsumeAsync("test");
		Assert.NotNull(token);
		Assert.Equal(7, token.UserId);
		Assert.Equal("test", token.Id);
		Assert.Null(await repository.ConsumeAsync("test"));
		Assert.Null(await repository.ConsumeAsync("missing"));
		Assert.Equal(new[] { "TgToken:test", "TgToken:test", "TgToken:missing" }, stub.ConsumedKeys);
	}

	public class RedisDatabaseStub : DispatchProxy
	{
		public Dictionary<string, RedisValue> Values { get; } = [];
		public List<string> ConsumedKeys { get; } = [];

		protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
		{
			if (targetMethod!.Name != nameof(IDatabase.StringGetDeleteAsync))
				throw new InvalidOperationException($"Unexpected Redis operation: {targetMethod.Name}");

			var key = args![0]!.ToString()!;
			ConsumedKeys.Add(key);
			var removed = Values.Remove(key, out var value);
			if (!removed)
				value = RedisValue.Null;

			return Task.FromResult(value);
		}
	}
}
