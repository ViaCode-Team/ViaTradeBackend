using Application.Common.Models.Filters;
using Application.Common.Models.Sort;
using Domain.Strategies.Entities;
using Domain.Strategies.Enums;

namespace Application.Common.Specifications;

public class StrategyQuerySpecification : BaseQuerySpecification<TradeStrategy>
{
	public StrategyQuerySpecification(int userId, StrategyFilterRequest? filter, StrategySortRequest? sort)
	{
		if (filter?.IsActive is bool isActive)
		{
			if (isActive)
				AddCriteria(x => x.UserTradeStrategies!.Any(uts => uts.UserId == userId));
			else
				AddCriteria(x => !x.UserTradeStrategies!.Any(uts => uts.UserId == userId));
		}

		if (sort?.SortBy != null && sort.SortBy.Count > 0)
		{
			foreach (var field in sort.SortBy)
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
}
