using ViaTrade.Application.Common.Models;
using ViaTrade.Application.Common.Specifications;
using ViaTrade.Domain.Entities;

namespace ViaTrade.Application.Reminders.Specifications;

public sealed class RemindsSearchSpecification(int userId, SearchFilter filter)
	: SearchSpecification<Reminder, SearchFilter>(filter)
{
	private readonly int _userId = userId;

	public override IQueryable<Reminder> Apply(IQueryable<Reminder> query)
	{
		query = query.Where(x => x.UserId == _userId);

		if (string.IsNullOrWhiteSpace(Filter.SearchText))
			return query;

		var searchText = Filter.SearchText;
		var isDate = DateTime.TryParse(searchText, out var date);

		query = query.Where(x =>
			(isDate && x.RemindAt.Date == date.Date)
			|| (x.Text != null && x.Text.Contains(searchText))
			|| (
				x.Instrument != null
				&& (
					(x.Instrument.Symbol != null && x.Instrument.Symbol.Contains(searchText))
					|| (x.Instrument.Description != null && x.Instrument.Description.Contains(searchText))
				)
			)
		);

		return query;
	}
}
