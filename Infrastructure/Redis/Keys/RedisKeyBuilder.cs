using StackExchange.Redis;

namespace ViaTrade.Infrastructure.Redis.Keys;

public sealed class RedisKeyBuilder
{
	private const char Separator = ':';

	private readonly string _scope;

	private RedisKeyBuilder(string scope)
	{
		_scope = scope;
	}

	public string Prefix => $"{_scope}{Separator}";

	public string Pattern => $"{_scope}{Separator}*";

	public static RedisKeyBuilder Create(string segment)
	{
		ValidateSegment(segment);

		return new RedisKeyBuilder(segment);
	}

	public RedisKeyBuilder Append(string segment)
	{
		ValidateSegment(segment);

		return new RedisKeyBuilder($"{_scope}{Separator}{segment}");
	}

	public RedisKey Build(string identifier) => $"{_scope}{Separator}{identifier}";

	public RedisKey Build(int identifier) => $"{_scope}{Separator}{identifier}";

	private static void ValidateSegment(string segment)
	{
		if (string.IsNullOrWhiteSpace(segment))
			throw new ArgumentException("Redis key segment cannot be empty.", nameof(segment));

		if (segment.Contains(Separator))
			throw new ArgumentException("Redis key segment cannot contain the separator.", nameof(segment));
	}
}
