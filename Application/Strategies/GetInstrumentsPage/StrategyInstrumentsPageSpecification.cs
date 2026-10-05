using Ardalis.Specification;
using ViaTrade.Application.Common.Models;
using ViaTrade.Application.Instruments.Common;
using ViaTrade.Application.Strategies.Common;

namespace ViaTrade.Application.Strategies.GetInstrumentsPage;

public sealed class StrategyInstrumentsPageSpecification : UserStrategyLinksSpecification
{
	public StrategyInstrumentsPageSpecification(
		int userId,
		int strategyId,
		StrategyInstrumentFilter instrumentFilter,
		PageOptions pageOptions,
		InstrumentSort instrumentSort
	)
		: base(userId, pageOptions)
	{
		Query.Where(link => link.StrategyId == strategyId);

		ApplyFilter(instrumentFilter);

		ApplySorting(instrumentSort);

		AddOrderByAscending(entity => entity.Id);
	}

	private void ApplyFilter(StrategyInstrumentFilter instrumentFilter)
	{
		if (instrumentFilter.InstrumentIds is { Count: > 0 })
			Query.Where(link => instrumentFilter.InstrumentIds.Contains(link.InstrumentId));
	}

	private void ApplySorting(InstrumentSort instrumentSort)
	{
		foreach (var field in instrumentSort.GetEffectiveSortBy())
		{
			switch (field)
			{
				case InstrumentSortField.SymbolDesc:
					AddOrderByDescending(link => link.Instrument!.Symbol);
					break;
				case InstrumentSortField.SymbolAsc:
					AddOrderByAscending(link => link.Instrument!.Symbol);
					break;
			}
		}
	}
}
