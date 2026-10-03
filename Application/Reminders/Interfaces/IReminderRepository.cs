namespace ViaTrade.Application.Reminders.Interfaces;

public interface IReminderRepository
{
	Task<int> ExecuteUpdateForUserAsync(
		int userId,
		int reminderId,
		string text,
		DateTime remindAt,
		CancellationToken ct = default
	);
	Task<int> ExecuteMarkPublishedAsync(int userId, int reminderId, CancellationToken ct = default);
	Task<int> ExecuteMarkDeliveredForUserAsync(int userId, int reminderId, CancellationToken ct = default);
}
