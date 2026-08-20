using ViaTrade.Application.Common.QueryObjects;
using ViaTrade.Application.Strategies.Models;
using ViaTrade.Domain.Entities;

namespace ViaTrade.Application.Strategies.QueryObjects;

public class UserStrategyInstrumentByInstrumentQueryObject : BaseQueryObject<UserStrategyInstrument>
{
	public UserStrategyInstrumentByInstrumentQueryObject(
		int userId,
		int instrumentId,
		StrategyFilter strategyFilter,
		StrategySort strategySort
	)
	{
		AddCriteria(link => link.UserId == userId && link.InstrumentId == instrumentId);

		ApplyFilter(strategyFilter);

		ApplySorting(strategySort);
	}

	private void ApplyFilter(StrategyFilter strategyFilter)
	{
		if (!string.IsNullOrWhiteSpace(strategyFilter.Name))
			AddCriteria(link => link.Strategy!.Name == strategyFilter.Name);
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
