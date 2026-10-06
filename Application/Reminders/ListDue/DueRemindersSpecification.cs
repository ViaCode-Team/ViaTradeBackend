using Ardalis.Specification;
using ViaTrade.Domain.Entities;

namespace ViaTrade.Application.Reminders.ListDue;

public sealed class DueRemindersSpecification : Specification<Reminder, DueReminderResult>
{
	public DueRemindersSpecification(int limit, DateTime utcNow)
	{
		Query
			.Where(reminder =>
				reminder.RemindAt <= utcNow && reminder.PublishedAt == null && reminder.User!.TelegramId != null
			)
			.OrderBy(reminder => reminder.RemindAt)
			.ThenBy(reminder => reminder.Id);
		Query.Take(limit);
		Query.Select(DueReminderResult.Projection);
	}
}
