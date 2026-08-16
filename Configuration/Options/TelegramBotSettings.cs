using System.ComponentModel.DataAnnotations;

namespace ViaTrade.Configuration.Options;

public sealed class TelegramBotSettings
{
	[Required]
	public string BotUsername { get; set; } = string.Empty;
}
