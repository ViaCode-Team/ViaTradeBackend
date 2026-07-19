using Application.Common.Models.Sort;
using Domain.Reminds.Entities;
using Domain.Reminds.Enums;

namespace Application.Common.Specifications;

public class TradeRemindQuerySpecification : BaseQuerySpecification<TradeRemind>
{
	public TradeRemindQuerySpecification(int userId, int? tradeCodeId = null, RemindSortRequest? sort = null)
	{
		AddCriteria(r => r.UserId == userId);

		if (tradeCodeId.HasValue)
		{
			AddCriteria(r => r.TradeCodeId == tradeCodeId.Value);
		}

		if (sort != null)
		{
			var sortFields = sort.GetEffectiveSortBy();
			foreach (var field in sortFields)
			{
				switch (field)
				{
					case RemindSortField.DateTimeAsc:
						AddOrderBy(r => r.DateTime, false);
						break;
					case RemindSortField.DateTimeDesc:
					default:
						AddOrderBy(r => r.DateTime, true);
						break;
				}
			}
		}

	}
}
