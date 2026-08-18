using ViaTrade.Application.Common.Specifications;
using ViaTrade.Application.Instruments.Models;
using ViaTrade.Domain.Entities;

namespace ViaTrade.Application.Instruments.Specifications;

public class InstrumentQuerySpecification : BaseQuerySpecification<Instrument>
{
	public InstrumentQuerySpecification(
		InstrumentFilter instrumentFilter,
		InstrumentSearch instrumentSearch,
		InstrumentSort instrumentSort
	)
	{
		var isSymbolFiltered = !string.IsNullOrWhiteSpace(instrumentFilter.Symbol);

		ApplyFilter(instrumentFilter, isSymbolFiltered);

		ApplySearch(instrumentSearch, isSymbolFiltered);

		ApplySorting(instrumentSort);
	}

	private void ApplyFilter(InstrumentFilter instrumentFilter, bool isSymbolFiltered)
	{
		if (isSymbolFiltered)
			AddCriteria(x => x.Symbol == instrumentFilter.Symbol);
	}

	private void ApplySearch(InstrumentSearch instrumentSearch, bool excludeSymbol)
	{
		var searchText = instrumentSearch.GetNormalizedSearchText();
		if (searchText == null)
			return;

		AddCriteria(x =>
			(excludeSymbol || x.Symbol.Contains(searchText))
			|| (x.Description != null && x.Description.Contains(searchText))
		);
	}

	private void ApplySorting(InstrumentSort sort)
	{
		foreach (var field in sort.GetEffectiveSortBy())
		{
			switch (field)
			{
				case InstrumentSortField.SymbolDesc:
					AddOrderBy(x => x.Symbol, descending: true);
					break;
				default:
					AddOrderBy(x => x.Symbol);
					break;
			}
		}
	}
}
