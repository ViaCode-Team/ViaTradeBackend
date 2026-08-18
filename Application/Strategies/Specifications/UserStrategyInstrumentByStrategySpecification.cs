using ViaTrade.Application.Common.Specifications;
using ViaTrade.Application.Instruments.Models;
using ViaTrade.Application.Strategies.Models;
using ViaTrade.Domain.Entities;

namespace ViaTrade.Application.Strategies.Specifications;

public class UserStrategyInstrumentByStrategySpecification : BaseQuerySpecification<UserStrategyInstrument>
{
	public UserStrategyInstrumentByStrategySpecification(
		int userId,
		int strategyId,
		StrategyInstrumentFilter instrumentFilter,
		InstrumentSort instrumentSort
	)
	{
		AddCriteria(link => link.UserId == userId && link.StrategyId == strategyId);

		if (instrumentFilter.InstrumentIds is { Count: > 0 })
			AddCriteria(link => instrumentFilter.InstrumentIds.Contains(link.InstrumentId));

		foreach (var field in instrumentSort.GetEffectiveSortBy())
		{
			switch (field)
			{
				case InstrumentSortField.SymbolDesc:
					AddOrderBy(link => link.Instrument!.Symbol, true);
					break;
				default:
					AddOrderBy(link => link.Instrument!.Symbol);
					break;
			}
		}
	}
}
