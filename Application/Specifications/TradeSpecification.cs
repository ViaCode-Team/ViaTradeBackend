using Domain.Entities.DataBase;
using Domain.Enums;
using Domain.Models.Filters;

namespace Application.Specifications;

public class TradeSpecification : BaseSpecification<Trade>
{
	public TradeSpecification(int userId, TradeFilterRequest? request)
	{
		ApplyNoTracking();
		AddCriteria(x => x.UserId == userId);
		AddInclude(x => x.TradeType!);
		AddInclude(x => x.TradeCode!);

		if (request == null) return;

		if (request.Signal.HasValue)
			AddCriteria(x => x.TradeSignal == request.Signal.Value);

		if (request.Status is TradeStatus status)
		{
			if (status == TradeStatus.Open)
				AddCriteria(x => x.DateClose == null);
			else if (status == TradeStatus.Closed)
				AddCriteria(x => x.DateClose != null);
		}

		if (!string.IsNullOrEmpty(request.TradeTypeName))
			AddCriteria(x => x.TradeType != null && x.TradeType.Name == request.TradeTypeName);

		if (request.StartDate.HasValue)
			AddCriteria(x => x.DateOpen >= request.StartDate.Value);

		if (request.EndDate.HasValue)
			AddCriteria(x => x.DateOpen <= request.EndDate.Value);
	}
}
