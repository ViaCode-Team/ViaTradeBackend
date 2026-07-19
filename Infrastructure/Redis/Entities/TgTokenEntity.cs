using Application.Common.Models;

namespace Infrastructure.Redis.Entities;

public class TgTokenEntity : CacheEntity
{
	public int UserId { get; set; }
}
