using Ardalis.Specification;
using ViaTrade.Application.Common.Models;
using ViaTrade.Application.Common.Specifications;
using ViaTrade.Domain.Entities;

namespace ViaTrade.Application.Reminders.Common;

public class RemindersPageSpecification : PageSpecification<Reminder>
{
	public RemindersPageSpecification(
		int userId,
		ReminderFilter reminderFilter,
		ReminderSearch reminderSearch,
		PageOptions pageOptions,
		ReminderSort reminderSort,
		int? instrumentId = null
	)
		: base(pageOptions)
	{
		Query.Where(r => r.UserId == userId);

		ApplyFilter(reminderFilter, instrumentId);

		ApplySearch(reminderSearch);

		ApplySorting(reminderSort);

		AddOrderByAscending(entity => entity.Id);
	}

	private void ApplyFilter(ReminderFilter reminderFilter, int? instrumentId)
	{
		if (instrumentId.HasValue)
			Query.Where(r => r.InstrumentId == instrumentId.Value);

		var deliveryStatus = reminderFilter.DeliveryStatus;

		if (!deliveryStatus.HasValue)
			return;

		switch (deliveryStatus.Value)
		{
			case ReminderDeliveryStatus.Undelivered:
				Query.Where(r => r.DeliveredAt == null);
				break;
			case ReminderDeliveryStatus.Delivered:
				Query.Where(r => r.DeliveredAt != null);
				break;
		}
	}

	private void ApplySearch(ReminderSearch reminderSearch)
	{
		var searchText = reminderSearch.GetNormalizedSearchText();
		if (searchText == null)
			return;

		var isDate = DateTime.TryParse(searchText, out var date);

		DateTime nextDay = default;
		if (isDate)
			nextDay = date.Date.AddDays(1);

		Query.Where(x =>
			(isDate && x.RemindAt >= date.Date && x.RemindAt < nextDay)
			|| x.Text.Contains(searchText)
			|| x.Instrument!.Symbol.Contains(searchText)
			|| x.Instrument.Description!.Contains(searchText)
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
