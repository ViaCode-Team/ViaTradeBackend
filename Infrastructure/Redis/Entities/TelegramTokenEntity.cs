using Application.Common.Models;

namespace Infrastructure.Redis.Entities;

public class TelegramTokenEntity : CacheEntity
{
	public int UserId { get; set; }
}
