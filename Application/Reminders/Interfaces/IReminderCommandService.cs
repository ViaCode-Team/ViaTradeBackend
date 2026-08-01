using Domain.Entities;

namespace Application.Reminders.Interfaces;

public interface IReminderCommandService
{
	Task<Reminder> CreateAsync(int userId, int instrumentId, string text, DateTime remindAt, CancellationToken ct);

	Task UpdateAsync(int userId, int reminderId, string text, DateTime remindAt, CancellationToken ct);

	Task DeleteAsync(int userId, int reminderId, CancellationToken ct);

	Task DeleteDueAsync(int reminderId, CancellationToken ct);
}
