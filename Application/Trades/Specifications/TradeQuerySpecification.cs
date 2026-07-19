using Application.Common.Models.Filters;
using Domain.Trades.Entities;
using Domain.Trades.Enums;

namespace Application.Common.Specifications;

public class TradeQuerySpecification : BaseQuerySpecification<Trade>
{
	public TradeQuerySpecification(int userId, TradeFilterRequest request)
	{
		AddCriteria(x => x.UserId == userId);
		AddInclude(x => x.TradeType!);
		AddInclude(x => x.TradeCode!);

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
