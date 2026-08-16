using System.ComponentModel.DataAnnotations;

namespace ViaTrade.Configuration.Options;

public sealed class NotificationStreamSettings
{
	[Range(0, int.MaxValue)]
	public int RedisDatabase { get; set; }

	[Required]
	public string StreamName { get; set; } = string.Empty;

	[Range(1, int.MaxValue)]
	public int MaxLength { get; set; }

	[Range(1, int.MaxValue)]
	public int ReminderPublishIntervalSeconds { get; set; }
}
