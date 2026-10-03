using Ardalis.Specification;
using ViaTrade.Application.Common.Models;
using ViaTrade.Application.Common.Specifications;
using ViaTrade.Application.Strategies.Models;
using ViaTrade.Domain.Entities;

namespace ViaTrade.Application.Strategies.Specifications;

public class StrategiesPageSpecification : PageSpecification<Strategy>
{
	public StrategiesPageSpecification(
		StrategyFilter strategyFilter,
		StrategySearch strategySearch,
		PageOptions pageOptions,
		StrategySort strategySort
	)
		: base(pageOptions)
	{
		ApplyFilter(strategyFilter);

		ApplySearch(strategySearch);

		ApplySorting(strategySort);

		AddOrderByAscending(entity => entity.Id);
	}

	private void ApplyFilter(StrategyFilter strategyFilter)
	{
		if (!string.IsNullOrWhiteSpace(strategyFilter.Name))
			Query.Where(x => x.Name == strategyFilter.Name);
	}

	private void ApplySearch(StrategySearch strategySearch)
	{
		var searchText = strategySearch.GetNormalizedSearchText();
		if (searchText == null)
			return;

		Query.Where(x =>
			x.Name.Contains(searchText)
			|| x.DisplayName.Contains(searchText)
			|| x.SignalFrequency!.Contains(searchText)
			|| x.InvestmentHorizon!.Contains(searchText)
		);
	}

	private void ApplySorting(StrategySort strategySort)
	{
		foreach (var field in strategySort.GetEffectiveSortBy())
		{
			switch (field)
			{
				case StrategySortField.NameAsc:
					AddOrderByAscending(x => x.Name);
					break;
				case StrategySortField.NameDesc:
					AddOrderByDescending(x => x.Name);
					break;
				case StrategySortField.AccuracyAsc:
					AddOrderByAscending(x => x.Accuracy ?? 0);
					break;
				case StrategySortField.AccuracyDesc:
					AddOrderByDescending(x => x.Accuracy ?? 0);
					break;
			}
		}
	}
}
