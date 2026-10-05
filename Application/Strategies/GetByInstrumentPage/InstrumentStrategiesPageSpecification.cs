using Ardalis.Specification;
using ViaTrade.Application.Common.Models;
using ViaTrade.Application.Strategies.Common;

namespace ViaTrade.Application.Strategies.GetByInstrumentPage;

public sealed class InstrumentStrategiesPageSpecification : UserStrategyLinksSpecification
{
	public InstrumentStrategiesPageSpecification(
		int userId,
		int instrumentId,
		StrategyFilter strategyFilter,
		PageOptions pageOptions,
		StrategySort strategySort
	)
		: base(userId, pageOptions)
	{
		Query.Where(link => link.InstrumentId == instrumentId);

		ApplyFilter(strategyFilter);

		ApplySorting(strategySort);

		AddOrderByAscending(entity => entity.Id);
	}

	private void ApplyFilter(StrategyFilter strategyFilter)
	{
		if (!string.IsNullOrWhiteSpace(strategyFilter.Name))
			Query.Where(link => link.Strategy!.Name == strategyFilter.Name);
	}

	private void ApplySorting(StrategySort strategySort)
	{
		foreach (var field in strategySort.GetEffectiveSortBy())
		{
			switch (field)
			{
				case StrategySortField.NameAsc:
					AddOrderByAscending(link => link.Strategy!.Name);
					break;
				case StrategySortField.NameDesc:
					AddOrderByDescending(link => link.Strategy!.Name);
					break;
				case StrategySortField.AccuracyAsc:
					AddOrderByAscending(link => link.Strategy!.Accuracy ?? 0);
					break;
				case StrategySortField.AccuracyDesc:
					AddOrderByDescending(link => link.Strategy!.Accuracy ?? 0);
					break;
			}
		}
	}
}
