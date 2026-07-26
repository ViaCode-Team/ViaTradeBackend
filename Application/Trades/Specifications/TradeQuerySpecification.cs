using Application.Trades.Models;
using Domain.Entities;
using Domain.Enums;

namespace Application.Common.Specifications;

public class TradeQuerySpecification : BaseQuerySpecification<Trade>
{
	public TradeQuerySpecification(int userId, TradeFilter request)
	{
		AddCriteria(x => x.UserId == userId);
		if (request.Signal.HasValue)
			AddCriteria(x => x.Signal == request.Signal.Value);

		if (request.Status is TradeStatus status)
		{
			if (status == TradeStatus.Open)
				AddCriteria(x => x.ClosedAt == null);
			else if (status == TradeStatus.Closed)
				AddCriteria(x => x.ClosedAt != null);
		}

		if (!string.IsNullOrEmpty(request.TradeTypeName))
			AddCriteria(x => x.TradeType != null && x.TradeType.Name == request.TradeTypeName);

		if (request.StartDate.HasValue)
			AddCriteria(x => x.OpenedAt >= request.StartDate.Value);

		if (request.EndDate.HasValue)
			AddCriteria(x => x.OpenedAt <= request.EndDate.Value);
	}
}
