namespace Application.Common.Models;

public abstract class RedisEntity
{
	public string Id { get; set; } = Guid.NewGuid().ToString();
}
