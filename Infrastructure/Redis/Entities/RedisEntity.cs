namespace Infrastructure.Redis.Entities;

public abstract class RedisEntity
{
	public string Id { get; set; } = Guid.NewGuid().ToString();
}
