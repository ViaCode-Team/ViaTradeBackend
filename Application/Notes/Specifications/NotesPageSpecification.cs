using Ardalis.Specification;
using ViaTrade.Application.Common.Models;
using ViaTrade.Application.Common.Specifications;
using ViaTrade.Application.Notes.Models;
using ViaTrade.Domain.Entities;
using ViaTrade.Domain.Enums;

namespace ViaTrade.Application.Notes.Specifications;

public class NotesPageSpecification : PageSpecification<Note>
{
	public NotesPageSpecification(int userId, NoteFilter noteFilter, NoteSearch noteSearch, PageOptions pageOptions)
		: base(pageOptions)
	{
		Query.Where(x => x.UserId == userId);

		ApplyFilter(noteFilter);

		ApplySearch(noteSearch);

		AddOrderByAscending(entity => entity.Id);
	}

	private void ApplyFilter(NoteFilter noteFilter)
	{
		if (noteFilter.Target is not { } target)
			return;

		switch (target)
		{
			case NoteType.InstrumentNote:
				Query.Where(x => x.InstrumentId != null);
				break;

			case NoteType.StrategyNote:
				Query.Where(x => x.StrategyId != null);
				break;
		}
	}

	private void ApplySearch(NoteSearch noteSearch)
	{
		var searchText = noteSearch.GetNormalizedSearchText();
		if (searchText == null)
			return;

		Query.Where(x =>
			x.Text.Contains(searchText)
			|| x.Instrument!.Symbol.Contains(searchText)
			|| x.Instrument.Description!.Contains(searchText)
			|| x.Strategy!.Name.Contains(searchText)
		);
	}
}
