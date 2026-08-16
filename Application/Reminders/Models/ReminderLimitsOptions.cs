namespace Application.Reminders.Models;

public sealed class ReminderLimitsOptions
{
	public int MaxRemindersPerUser { get; init; } = 3_000;
}
