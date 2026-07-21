using Application.Common.Interfaces.Repositories;
using Application.Common.Models;
using Application.Reminders.Models;
using Domain.Reminders.Entities;

namespace Application.Reminders.Interfaces;

public interface IReminderRepository : IRepository<Reminder>
{
	Task<IReadOnlyList<Reminder>> ListDueAsync(CancellationToken ct = default);
	Task<int> CountByUserAsync(int userId, CancellationToken ct = default);
	Task<int> ExecuteUpdateForUserAsync(
		int userId,
		int reminderId,
		string text,
		DateTime dateTime,
		CancellationToken ct = default
	);
}
