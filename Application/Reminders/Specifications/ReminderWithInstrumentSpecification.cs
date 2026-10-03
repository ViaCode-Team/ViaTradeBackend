using Ardalis.Specification;
using ViaTrade.Domain.Entities;

namespace ViaTrade.Application.Reminders.Specifications;

public sealed class ReminderWithInstrumentSpecification : Specification<Reminder>
{
	public ReminderWithInstrumentSpecification(int userId, int reminderId)
	{
		Query
			.Where(reminder => reminder.UserId == userId && reminder.Id == reminderId)
			.Include(reminder => reminder.Instrument);
	}
}
