using Domain.Entities.Redis;

namespace Application.Intarfaces
{
    public interface IRedisRepository<T> where T : RedisEntity
    {
        Task<T?> GetAsync(string id);
        Task SetAsync(T entity, TimeSpan? expiry = null);
        Task RemoveAsync(string id);
        Task<IEnumerable<T>> GetAllAsync();
    }
}
