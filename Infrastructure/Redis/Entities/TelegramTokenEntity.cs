using ViaTrade.Application.Common.Models;

namespace ViaTrade.Infrastructure.Redis.Entities;

public class TelegramTokenEntity : CacheEntity
{
	public int UserId { get; set; }
}
