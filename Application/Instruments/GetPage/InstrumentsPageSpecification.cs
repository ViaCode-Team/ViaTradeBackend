using Ardalis.Specification;
using ViaTrade.Application.Common.Models;
using ViaTrade.Application.Common.Specifications;
using ViaTrade.Application.Instruments.Common;
using ViaTrade.Domain.Entities;

namespace ViaTrade.Application.Instruments.GetPage;

public class InstrumentsPageSpecification : PageSpecification<Instrument>
{
	public InstrumentsPageSpecification(
		InstrumentFilter instrumentFilter,
		InstrumentSearch instrumentSearch,
		PageOptions pageOptions,
		InstrumentSort instrumentSort
	)
		: base(pageOptions)
	{
		ApplyFilter(instrumentFilter);

		ApplySearch(instrumentSearch);

		ApplySorting(instrumentSort);

		AddOrderByAscending(entity => entity.Id);
	}

	private void ApplyFilter(InstrumentFilter instrumentFilter)
	{
		if (!string.IsNullOrWhiteSpace(instrumentFilter.Ticker))
			Query.Where(x => x.Ticker == instrumentFilter.Ticker);
	}

	private void ApplySearch(InstrumentSearch instrumentSearch)
	{
		var searchText = instrumentSearch.GetNormalizedSearchText();
		if (searchText == null)
			return;

		Query.Where(x => x.Ticker.Contains(searchText) || x.Description!.Contains(searchText));
	}

	private void ApplySorting(InstrumentSort sort)
	{
		foreach (var field in sort.GetEffectiveSortBy())
		{
			switch (field)
			{
				case InstrumentSortField.TickerDesc:
					AddOrderByDescending(x => x.Ticker);
					break;
				case InstrumentSortField.TickerAsc:
					AddOrderByAscending(x => x.Ticker);
					break;
			}
		}
	}
}
