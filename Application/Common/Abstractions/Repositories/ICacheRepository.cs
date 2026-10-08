using ViaTrade.Application.Common.Models;

namespace ViaTrade.Application.Common.Abstractions.Repositories;

public interface ICacheRepository<T>
	where T : CacheEntity
{
	Task<T?> FindByIdAsync(string id);
	Task<T?> ConsumeAsync(string id);
	Task SetAsync(T entity, TimeSpan? expiry = null);
	Task RemoveAsync(string id);
	Task<IReadOnlyList<T>> ListAsync();
}
