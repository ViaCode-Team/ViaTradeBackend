using ViaTrade.Application.Common.Models;

namespace ViaTrade.Application.Users.Models;

public class TelegramTokenEntity : CacheEntity
{
	public int UserId { get; set; }
}
