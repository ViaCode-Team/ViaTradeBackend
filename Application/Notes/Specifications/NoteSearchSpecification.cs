using ViaTrade.Application.Common.Models;
using ViaTrade.Application.Common.Specifications;
using ViaTrade.Domain.Entities;


namespace ViaTrade.Application.Notes.Specifications;

public sealed class NoteSearchSpecification(int userId, SearchFilter filter)
	: SearchSpecification<Note, SearchFilter>(filter)
{
	private readonly int _userId = userId;

	public override IQueryable<Note> Apply(IQueryable<Note> query)
	{
		query = query.Where(x => x.UserId == _userId);

		if (!string.IsNullOrWhiteSpace(Filter.SearchText))
		{
			var searchText = Filter.SearchText;
			query = query.Where(x =>
				(x.Text != null && x.Text.Contains(searchText)) ||
				(x.Instrument != null &&
					((x.Instrument.Symbol != null && x.Instrument.Symbol.Contains(searchText)) ||
					 (x.Instrument.Description != null && x.Instrument.Description.Contains(searchText)))) ||
				(x.Strategy != null && x.Strategy.Name != null && x.Strategy.Name.Contains(searchText)));
		}

		return query;
	}
}
