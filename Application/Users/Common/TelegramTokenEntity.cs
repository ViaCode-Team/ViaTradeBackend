using ViaTrade.Application.Common.Models;

namespace ViaTrade.Application.Users.Common;

public class TelegramTokenEntity : CacheEntity
{
	public int UserId { get; set; }
}
