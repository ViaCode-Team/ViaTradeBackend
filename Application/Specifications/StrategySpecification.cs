using Application.Specifications;
using Domain.Entities.DataBase;
using Domain.Models.Filters;

namespace Application.Specifications;

public class StrategySpecification : BaseSpecification<TradeStrategy>
{
	public StrategySpecification(int userId, StrategyFilterRequest? request)
	{
		ApplyNoTracking();

		if (request == null) return;

		if (request.IsActive is bool isActive)
		{
			if (isActive)
				AddCriteria(x => x.UserTradeStrategies != null && x.UserTradeStrategies.Any(uts => uts.UserId == userId));
			else
				AddCriteria(x => x.UserTradeStrategies == null || !x.UserTradeStrategies.Any(uts => uts.UserId == userId));
		}
	}
}
