using System.ComponentModel.DataAnnotations;

namespace ViaTrade.Configuration.Options;

public sealed class ReminderCleanupSettings
{
	[Range(1, int.MaxValue)]
	public int RetentionDays { get; set; }

	[Range(1, int.MaxValue)]
	public int CleanupIntervalHours { get; set; }
}
