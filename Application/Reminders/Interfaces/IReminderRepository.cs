using Application.Common.Interfaces.Repositories;
using Application.Common.Models;
using Application.Reminders.Models;
using Domain.Reminders.Entities;

namespace Application.Reminders.Interfaces;

public interface IReminderRepository : IRepository<Reminder>
{
	Task<IEnumerable<Reminder>> GetDueRemindersAsync(CancellationToken ct = default);
	Task<int> CountByUserAsync(int userId, CancellationToken ct = default);
	Task<int> UpdateForUserAsync(
		int reminderId,
		int userId,
		string text,
		DateTime dateTime,
		CancellationToken ct = default
	);
}
