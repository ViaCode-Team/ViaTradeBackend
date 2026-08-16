using System.ComponentModel.DataAnnotations;

namespace ViaTrade.Configuration.Options;

public sealed class ReminderLimitsSettings
{
	[Range(1, int.MaxValue)]
	public int MaxRemindersPerUser { get; set; }
}
