using Application.Common.Models;

namespace Infrastructure.Redis.Entities;

public class UserRedisEntity : CacheEntity
{
	public string Login { get; set; } = default!;

	public string? RefreshToken { get; set; }

	public DateTime LastLogin { get; set; }
}
