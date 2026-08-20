using ViaTrade.Application.Common.QueryObjects;
using ViaTrade.Application.Reminders.Models;
using ViaTrade.Domain.Entities;

namespace ViaTrade.Application.Reminders.QueryObjects;

public class ReminderQueryObject : BaseQueryObject<Reminder>
{
	public ReminderQueryObject(
		ReminderFilter reminderFilter,
		ReminderSearch reminderSearch,
		ReminderSort reminderSort,
		int userId,
		int? instrumentId = null
	)
	{
		AddCriteria(r => r.UserId == userId);

		ApplyFilter(reminderFilter, instrumentId);

		ApplySearch(reminderSearch);

		ApplySorting(reminderSort);
	}

	private void ApplyFilter(ReminderFilter reminderFilter, int? instrumentId)
	{
		if (instrumentId.HasValue)
			AddCriteria(r => r.InstrumentId == instrumentId.Value);

		var deliveryStatus = reminderFilter.DeliveryStatus;

		if (!deliveryStatus.HasValue)
			return;

		switch (deliveryStatus.Value)
		{
			case ReminderDeliveryStatus.Undelivered:
				AddCriteria(r => r.DeliveredAt == null);
				break;
			case ReminderDeliveryStatus.Delivered:
				AddCriteria(r => r.DeliveredAt != null);
				break;
		}
	}

	private void ApplySearch(ReminderSearch reminderSearch)
	{
		var searchText = reminderSearch.GetNormalizedSearchText();
		if (searchText == null)
			return;

		var isDate = DateTime.TryParse(searchText, out var date);
		var nextDay = isDate ? date.Date.AddDays(1) : default;

		AddCriteria(x =>
			(isDate && x.RemindAt >= date.Date && x.RemindAt < nextDay)
			|| (x.Text.Contains(searchText))
			|| (
				x.Instrument != null
				&& (
					(x.Instrument.Symbol != null && x.Instrument.Symbol.Contains(searchText))
					|| (x.Instrument.Description != null && x.Instrument.Description.Contains(searchText))
				)
			)
		);
	}

	private void ApplySorting(ReminderSort reminderSort)
	{
		foreach (var field in reminderSort.GetEffectiveSortBy())
		{
			switch (field)
			{
				case ReminderSortField.RemindAtAsc:
					AddOrderByAscending(r => r.RemindAt);
					break;
				case ReminderSortField.RemindAtDesc:
				default:
					AddOrderByDescending(r => r.RemindAt);
					break;
			}
		}
	}
}
