using ViaTrade.Application.Common.QueryObjects;
using ViaTrade.Application.Instruments.Models;
using ViaTrade.Application.Strategies.Models;
using ViaTrade.Domain.Entities;

namespace ViaTrade.Application.Strategies.QueryObjects;

public class UserStrategyInstrumentByStrategyQueryObject : BaseQueryObject<UserStrategyInstrument>
{
	public UserStrategyInstrumentByStrategyQueryObject(
		int userId,
		int strategyId,
		StrategyInstrumentFilter instrumentFilter,
		InstrumentSort instrumentSort
	)
	{
		AddCriteria(link => link.UserId == userId && link.StrategyId == strategyId);

		ApplyFilter(instrumentFilter);

		ApplySorting(instrumentSort);
	}

	private void ApplyFilter(StrategyInstrumentFilter instrumentFilter)
	{
		if (instrumentFilter.InstrumentIds is { Count: > 0 })
			AddCriteria(link => instrumentFilter.InstrumentIds.Contains(link.InstrumentId));
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
