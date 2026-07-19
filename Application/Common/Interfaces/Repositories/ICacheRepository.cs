using Application.Common.Models;

namespace Application.Common.Interfaces.Repositories;

public interface ICacheRepository<T> where T : CacheEntity
{
	Task<T?> GetAsync(string id);
	Task SetAsync(T entity, TimeSpan? expiry = null);
	Task RemoveAsync(string id);
	Task<IEnumerable<T>> GetAllAsync();
}
