using Application.Common.Specifications;
using Application.Strategies.Models;
using Domain.Entities;

namespace Application.Strategies.Specifications;

public class StrategyQuerySpecification : BaseQuerySpecification<Strategy>
{
	public StrategyQuerySpecification(int userId, StrategyFilter strategyFilter, StrategySort strategySort)
	{
		if (!string.IsNullOrWhiteSpace(strategyFilter.Name))
			AddCriteria(x => x.Name == strategyFilter.Name);

		var sortFields = strategySort.GetEffectiveSortBy();
		foreach (var field in sortFields)
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
