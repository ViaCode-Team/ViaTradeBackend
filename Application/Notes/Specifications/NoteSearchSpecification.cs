using Application.Common.Specifications;
using Application.Notes.Models;
using Domain.Entities;

namespace Application.Notes.Specifications;

public sealed class NoteSearchSpecification(int userId, NoteSearchFilter filter)
	: SearchSpecification<Note, NoteSearchFilter>(filter)
{
	private readonly int _userId = userId;

	public override IQueryable<Note> Apply(IQueryable<Note> query)
	{
		query = query.Where(x => x.UserId == _userId);

		if (!string.IsNullOrWhiteSpace(Filter.Text))
			query = query.Where(x => x.Text.Contains(Filter.Text));

		if (!string.IsNullOrWhiteSpace(Filter.InstrumentSymbol))
			query = query.Where(x => x.Instrument != null
				&& x.Instrument.Symbol.Contains(Filter.InstrumentSymbol));

		if (!string.IsNullOrWhiteSpace(Filter.InstrumentDescription))
			query = query.Where(x => x.Instrument != null && x.Instrument.Description != null
				&& x.Instrument.Description.Contains(Filter.InstrumentDescription));

		if (!string.IsNullOrWhiteSpace(Filter.StrategyName))
			query = query.Where(x => x.Strategy != null
				&& x.Strategy.Name.Contains(Filter.StrategyName));

		return query;
	}
}
