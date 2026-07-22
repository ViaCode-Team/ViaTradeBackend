using Application.Reminders.Models;
using Domain.Reminders.Entities;

namespace Application.Common.Specifications;

public class ReminderQuerySpecification : BaseQuerySpecification<Reminder>
{
	public ReminderQuerySpecification(int userId, int? tradeCodeId, ReminderSort reminderSort)
	{
		AddCriteria(r => r.UserId == userId);

		if (tradeCodeId.HasValue)
		{
			AddCriteria(r => r.TradeCodeId == tradeCodeId.Value);
		}

		var sortFields = reminderSort.GetEffectiveSortBy();
		foreach (var field in sortFields)
		{
			switch (field)
			{
				case ReminderSortField.DateTimeAsc:
					AddOrderBy(r => r.DateTime, false);
					break;
				case ReminderSortField.DateTimeDesc:
				default:
					AddOrderBy(r => r.DateTime, true);
					break;
			}
		}
	}
}
