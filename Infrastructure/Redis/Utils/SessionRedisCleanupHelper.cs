using StackExchange.Redis;
using ViaTrade.Infrastructure.Redis.Keys;

namespace ViaTrade.Infrastructure.Redis.Utils;

internal sealed class SessionRedisCleanupHelper(IDatabase database)
{
	private const int BatchSize = 500;
	private const int MaxBatchCount = 10;

	private readonly IDatabase _database = database;

	public async Task<int> CleanupExpiredSessionsAsync(DateTime utcNow)
	{
		var totalDeleted = 0;

		for (var batchNumber = 0; batchNumber < MaxBatchCount; batchNumber++)
		{
			var expiredSessionBatch = await GetExpiredSessionBatchAsync(utcNow);
			if (expiredSessionBatch.MemberCount == 0)
				break;

			var staleIndexes = await FindStaleSessionIndexesAsync(expiredSessionBatch.ValidIndexes);
			var deletedCount = await RemoveStaleSessionIndexesAsync(expiredSessionBatch, staleIndexes);
			totalDeleted += deletedCount;

			if (deletedCount == 0 || expiredSessionBatch.MemberCount < BatchSize)
				break;
		}

		return totalDeleted;
	}

	private async Task<ExpiredSessionBatch> GetExpiredSessionBatchAsync(DateTime utcNow)
	{
		var expirationMembers = await _database.SortedSetRangeByScoreAsync(
			RedisKeys.Sessions.ExpirationIndex,
			double.NegativeInfinity,
			utcNow.Ticks,
			Exclude.None,
			Order.Ascending,
			0,
			BatchSize
		);
		List<ExpiredSessionIndex> validIndexes = [];
		List<RedisValue> invalidExpirationMembers = [];

		foreach (var expirationMember in expirationMembers)
		{
			var hasExpirationMember = RedisKeys.Sessions.TryParseExpirationMember(
				expirationMember,
				out var userId,
				out var sessionId
			);

			if (!hasExpirationMember)
			{
				invalidExpirationMembers.Add(expirationMember);
				continue;
			}

			validIndexes.Add(new ExpiredSessionIndex(userId, sessionId, expirationMember));
		}

		return new ExpiredSessionBatch(expirationMembers.Length, validIndexes, invalidExpirationMembers);
	}

	private async Task<List<ExpiredSessionIndex>> FindStaleSessionIndexesAsync(
		IReadOnlyList<ExpiredSessionIndex> indexes
	)
	{
		if (indexes.Count == 0)
			return [];

		var keys = indexes.Select(index => RedisKeys.Sessions.ById(index.SessionId)).ToArray();
		var sessionValues = await _database.StringGetAsync(keys);
		List<ExpiredSessionIndex> staleIndexes = [];

		for (var index = 0; index < indexes.Count; index++)
		{
			if (sessionValues[index].IsNullOrEmpty)
				staleIndexes.Add(indexes[index]);
		}

		return staleIndexes;
	}

	private async Task<int> RemoveStaleSessionIndexesAsync(
		ExpiredSessionBatch expiredSessionBatch,
		IReadOnlyList<ExpiredSessionIndex> staleIndexes
	)
	{
		var expirationMembers = expiredSessionBatch
			.InvalidExpirationMembers.Concat(staleIndexes.Select(index => index.ExpirationMember))
			.ToArray();
		if (expirationMembers.Length == 0)
			return 0;

		var batch = _database.CreateBatch();
		List<Task> removalTasks = [];
		var removeExpirationMembers = batch.SortedSetRemoveAsync(RedisKeys.Sessions.ExpirationIndex, expirationMembers);
		removalTasks.Add(removeExpirationMembers);

		foreach (var userIndexes in staleIndexes.GroupBy(index => index.UserId))
		{
			var sessionIds = userIndexes.Select(index => (RedisValue)index.SessionId).ToArray();
			removalTasks.Add(batch.SortedSetRemoveAsync(RedisKeys.Sessions.ByUser(userIndexes.Key), sessionIds));
		}

		batch.Execute();
		await Task.WhenAll(removalTasks);

		return (int)await removeExpirationMembers;
	}

	private readonly record struct ExpiredSessionIndex(int UserId, string SessionId, RedisValue ExpirationMember);

	private readonly record struct ExpiredSessionBatch(
		int MemberCount,
		IReadOnlyList<ExpiredSessionIndex> ValidIndexes,
		IReadOnlyList<RedisValue> InvalidExpirationMembers
	);
}
