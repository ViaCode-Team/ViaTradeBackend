using ViaTrade.Application.Common.QueryObjects;
using ViaTrade.Application.Notes.Models;
using ViaTrade.Domain.Entities;
using ViaTrade.Domain.Enums;

namespace ViaTrade.Application.Notes.QueryObjects;

public class NoteQueryObject : BaseQueryObject<Note>
{
	public NoteQueryObject(int userId, NoteFilter noteFilter, NoteSearch noteSearch)
	{
		AddCriteria(x => x.UserId == userId);

		ApplyFilter(noteFilter);

		ApplySearch(noteSearch);
	}

	private void ApplyFilter(NoteFilter noteFilter)
	{
		if (noteFilter.Target is not { } target)
			return;

		switch (target)
		{
			case NoteType.InstrumentNote:
				AddCriteria(x => x.InstrumentId != null);
				break;

			case NoteType.StrategyNote:
				AddCriteria(x => x.StrategyId != null);
				break;
		}
	}

	private void ApplySearch(NoteSearch noteSearch)
	{
		var searchText = noteSearch.GetNormalizedSearchText();
		if (searchText == null)
			return;

		AddCriteria(x =>
			(x.Text.Contains(searchText))
			|| (
				x.Instrument != null
				&& (
					(x.Instrument.Symbol != null && x.Instrument.Symbol.Contains(searchText))
					|| (x.Instrument.Description != null && x.Instrument.Description.Contains(searchText))
				)
			)
			|| (x.Strategy != null && x.Strategy.Name != null && x.Strategy.Name.Contains(searchText))
		);
	}
}
