using Application.Common.Models;
using Application.Common.Specifications;
using Application.Instruments.Models;
using Domain.Entities;

namespace Application.Instruments.Specifications;

public sealed class InstrumentSearchSpecification(SearchFilter filter)
	: SearchSpecification<Instrument, SearchFilter>(filter)
{
	public override IQueryable<Instrument> Apply(IQueryable<Instrument> query)
	{
		if (!string.IsNullOrWhiteSpace(Filter.SearchText))
		{
			var searchText = Filter.SearchText;
			query = query.Where(x =>
				(x.Symbol != null && x.Symbol.Contains(searchText)) ||
				(x.Description != null && x.Description.Contains(searchText)));
		}

		return query;
	}
}
