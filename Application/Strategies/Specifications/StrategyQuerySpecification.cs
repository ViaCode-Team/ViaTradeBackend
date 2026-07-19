using Application.Strategies.Queries;
using Domain.Strategies.Entities;

namespace Application.Common.Specifications;

public class StrategyQuerySpecification : BaseQuerySpecification<TradeStrategy>
{
	public StrategyQuerySpecification(int userId, StrategyFilter filter, StrategySort sort)
	{
		if (filter.IsActive is bool isActive)
		{
			if (isActive)
				AddCriteria(x => x.UserTradeStrategies!.Any(uts => uts.UserId == userId));
			else
				AddCriteria(x => !x.UserTradeStrategies!.Any(uts => uts.UserId == userId));
		}

		var sortFields = sort.GetEffectiveSortBy();
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
