using Application.Common.Specifications;
using Application.Trades.Models;
using Domain.Entities;
using Domain.Enums;

namespace Application.Trades.Specifications;

public class TradeQuerySpecification : BaseQuerySpecification<Trade>
{
	public TradeQuerySpecification(int userId, TradeFilter request)
	{
		AddCriteria(x => x.UserId == userId);

		if (request.Signal.HasValue)
			AddCriteria(x => x.Signal == request.Signal.Value);

		var status = request.Status;

		if (status.HasValue)
		{
			if (status.Value == TradeStatus.Open)
				AddCriteria(x => x.ClosedAt == null);
			else if (status.Value == TradeStatus.Closed)
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
