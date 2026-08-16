using ViaTrade.Application.Common.Specifications;
using ViaTrade.Application.Reminders.Models;
using ViaTrade.Domain.Entities;

namespace ViaTrade.Application.Reminders.Specifications;

public class ReminderQuerySpecification : BaseQuerySpecification<Reminder>
{
	public ReminderQuerySpecification(
		int userId,
		int? instrumentId,
		ReminderDeliveryStatus deliveryStatus,
		ReminderSort reminderSort
	)
	{
		AddCriteria(r => r.UserId == userId);

		if (instrumentId.HasValue)
			AddCriteria(r => r.InstrumentId == instrumentId.Value);

		switch (deliveryStatus)
		{
			case ReminderDeliveryStatus.Undelivered:
				AddCriteria(r => r.DeliveredAt == null);
				break;
			case ReminderDeliveryStatus.Delivered:
				AddCriteria(r => r.DeliveredAt != null);
				break;
		}

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
