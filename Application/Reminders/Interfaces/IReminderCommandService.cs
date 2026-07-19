namespace Application.Reminders.Interfaces;

public interface IReminderCommandService
{
	Task CreateAsync(
		int userId,
		int tradeCodeId,
		string textRemind,
		DateTime dateTime,
		CancellationToken ct);

	Task UpdateAsync(
		int remindId,
		int userId,
		string textRemind,
		DateTime dateTime,
		CancellationToken ct);

	Task DeleteAsync(
		int remindId,
		int userId,
		CancellationToken ct);

	Task DeleteAsync(
		int remindId,
		CancellationToken ct);
}
