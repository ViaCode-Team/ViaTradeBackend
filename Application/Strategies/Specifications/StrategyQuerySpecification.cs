using ViaTrade.Application.Common.Specifications;
using ViaTrade.Application.Strategies.Models;
using ViaTrade.Domain.Entities;

namespace ViaTrade.Application.Strategies.Specifications;

public class StrategyQuerySpecification : BaseQuerySpecification<Strategy>
{
	public StrategyQuerySpecification(
		StrategyFilter strategyFilter,
		StrategySearch strategySearch,
		StrategySort strategySort
	)
	{
		var isNameFiltered = !string.IsNullOrWhiteSpace(strategyFilter.Name);

		ApplyFilter(strategyFilter, isNameFiltered);

		ApplySearch(strategySearch, isNameFiltered);

		ApplySorting(strategySort);
	}

	private void ApplyFilter(StrategyFilter strategyFilter, bool isNameFiltered)
	{
		if (isNameFiltered)
			AddCriteria(x => x.Name == strategyFilter.Name);
	}

	private void ApplySearch(StrategySearch strategySearch, bool isNameFiltered)
	{
		var searchText = strategySearch.GetNormalizedSearchText();
		if (searchText == null)
			return;

		AddCriteria(x =>
			(!isNameFiltered && x.Name.Contains(searchText))
			|| x.DisplayName.Contains(searchText)
			|| (x.SignalFrequency != null && x.SignalFrequency.Contains(searchText))
			|| (x.InvestmentHorizon != null && x.InvestmentHorizon.Contains(searchText))
		);
	}

	private void ApplySorting(StrategySort strategySort)
	{
		foreach (var field in strategySort.GetEffectiveSortBy())
		{
			switch (field)
			{
				case StrategySortField.NameAsc:
					AddOrderBy(x => x.Name, false);
					break;
				case StrategySortField.NameDesc:
					AddOrderBy(x => x.Name, true);
					break;
				case StrategySortField.AccuracyAsc:
					AddOrderBy(x => x.Accuracy ?? 0, false);
					break;
				case StrategySortField.AccuracyDesc:
					AddOrderBy(x => x.Accuracy ?? 0, true);
					break;
			}
		}
	}
}
