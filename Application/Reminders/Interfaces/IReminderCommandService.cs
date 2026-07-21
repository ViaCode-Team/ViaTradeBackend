namespace Application.Reminders.Interfaces;

public interface IReminderCommandService
{
	Task CreateAsync(int userId, int tradeCodeId, string text, DateTime dateTime, CancellationToken ct);

	Task UpdateAsync(int userId, int reminderId, string text, DateTime dateTime, CancellationToken ct);

	Task DeleteAsync(int userId, int reminderId, CancellationToken ct);

	Task DeleteDueAsync(int reminderId, CancellationToken ct);
}
