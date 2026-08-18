using ViaTrade.Application.Common.Specifications;
using ViaTrade.Application.Strategies.Models;
using ViaTrade.Domain.Entities;

namespace ViaTrade.Application.Strategies.Specifications;

public class UserStrategyInstrumentByInstrumentSpecification : BaseQuerySpecification<UserStrategyInstrument>
{
	public UserStrategyInstrumentByInstrumentSpecification(
		int userId,
		int instrumentId,
		StrategyFilter strategyFilter,
		StrategySort strategySort
	)
	{
		AddCriteria(link => link.UserId == userId && link.InstrumentId == instrumentId);

		if (!string.IsNullOrWhiteSpace(strategyFilter.Name))
			AddCriteria(link => link.Strategy!.Name == strategyFilter.Name);

		foreach (var field in strategySort.GetEffectiveSortBy())
		{
			switch (field)
			{
				case StrategySortField.NameDesc:
					AddOrderBy(link => link.Strategy!.Name, true);
					break;
				case StrategySortField.AccuracyAsc:
					AddOrderBy(link => link.Strategy!.Accuracy ?? 0);
					break;
				case StrategySortField.AccuracyDesc:
					AddOrderBy(link => link.Strategy!.Accuracy ?? 0, true);
					break;
				default:
					AddOrderBy(link => link.Strategy!.Name);
					break;
			}
		}
	}
}
