using Domain.Entities.DataBase;
using Domain.Enums;
using Domain.Models.Filters;
using Domain.Models.Sort;

namespace Application.Specifications;

public class StrategySpecification : BaseSpecification<TradeStrategy>
{
	public StrategySpecification(int userId, StrategyFilterRequest? filter, StrategySortRequest? sort)
	{
		ApplyNoTracking();

		if (filter?.IsActive is bool isActive)
		{
			if (isActive)
				AddCriteria(x => x.UserTradeStrategies != null && x.UserTradeStrategies.Any(uts => uts.UserId == userId));
			else
				AddCriteria(x => x.UserTradeStrategies == null || !x.UserTradeStrategies.Any(uts => uts.UserId == userId));
		}

		switch (sort?.SortOrder ?? StrategySortOrder.NameAscending)
		{
			case StrategySortOrder.NameDescending:
				ApplyOrderByDescending(x => x.Name);
				break;
			case StrategySortOrder.AccuracyDescending:
				ApplyOrderByDescending(x => x.Accuracy ?? 0);
				break;
			case StrategySortOrder.AccuracyAscending:
				ApplyOrderBy(x => x.Accuracy ?? 0);
				break;
			default:
				ApplyOrderBy(x => x.Name);
				break;
		}
	}
}
