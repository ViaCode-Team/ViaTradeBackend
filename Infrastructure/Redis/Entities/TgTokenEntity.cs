using Application.Common.Models;

namespace Infrastructure.Redis.Entities;

public class TgTokenEntity : RedisEntity
{
	public int UserId { get; set; }
}
