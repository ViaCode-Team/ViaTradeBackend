using Application.Common.Specifications;
using Application.Reminders.Models;
using Domain.Entities;

namespace Application.Reminders.Specifications;

public class ReminderQuerySpecification : BaseQuerySpecification<Reminder>
{
	public ReminderQuerySpecification(int userId, int? instrumentId, ReminderSort reminderSort)
	{
		AddCriteria(r => r.UserId == userId);

		if (instrumentId.HasValue)
			AddCriteria(r => r.InstrumentId == instrumentId.Value);

		var sortFields = reminderSort.GetEffectiveSortBy();
		foreach (var field in sortFields)
		{
			switch (field)
			{
				case ReminderSortField.RemindAtAsc:
					AddOrderBy(r => r.RemindAt, false);
					break;
				case ReminderSortField.RemindAtDesc:
				default:
					AddOrderBy(r => r.RemindAt, true);
					break;
			}
		}
	}
}
