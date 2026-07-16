using Domain.Entities.DataBase;
using Domain.Enums;
using Domain.Models.Sort;

namespace Application.Specifications;

public class TradeRemindSpecification : BaseSpecification<TradeRemind>
{
	public TradeRemindSpecification(int userId, int? tradeCodeId = null, RemindSortRequest? sort = null)
	{
		ApplyNoTracking();

		AddCriteria(r => r.UserId == userId);

		if (tradeCodeId.HasValue)
		{
			AddCriteria(r => r.TradeCodeId == tradeCodeId.Value);
		}

		if (sort?.SortBy != null && sort.SortBy.Count > 0)
		{
			foreach (var field in sort.SortBy)
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
