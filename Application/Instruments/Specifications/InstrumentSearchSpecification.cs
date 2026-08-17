using ViaTrade.Application.Common.Models;
using ViaTrade.Application.Common.Specifications;
using ViaTrade.Domain.Entities;

namespace ViaTrade.Application.Instruments.Specifications;

public sealed class InstrumentSearchSpecification(SearchFilter filter)
	: SearchSpecification<Instrument, SearchFilter>(filter)
{
	public override IQueryable<Instrument> Apply(IQueryable<Instrument> query)
	{
		if (string.IsNullOrWhiteSpace(Filter.SearchText))
			return query;

		var searchText = Filter.SearchText;
		query = query.Where(x =>
			(x.Symbol != null && x.Symbol.Contains(searchText))
			|| (x.Description != null && x.Description.Contains(searchText))
		);

		return query;
	}
}
