namespace Application.Reminders.Models;

public sealed class ReminderCleanupOptions
{
	public int RetentionDays { get; init; } = 30;

	public int CleanupIntervalHours { get; init; } = 24;
}
