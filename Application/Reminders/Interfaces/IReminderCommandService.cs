namespace Application.Reminders.Interfaces;

public interface IReminderCommandService
{
	Task CreateAsync(int userId, int tradeCodeId, string text, DateTime dateTime, CancellationToken ct);

	Task UpdateAsync(int reminderId, int userId, string text, DateTime dateTime, CancellationToken ct);

	Task DeleteAsync(int reminderId, int userId, CancellationToken ct);

	Task DeleteAsync(int reminderId, CancellationToken ct);
}
