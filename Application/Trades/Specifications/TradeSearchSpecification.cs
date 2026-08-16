using Application.Common.Models;
using Application.Common.Specifications;
using Domain.Entities;

namespace Application.Trades.Specifications;

public sealed class TradeSearchSpecification(int userId, SearchFilter filter)
	: SearchSpecification<Trade, SearchFilter>(filter)
{
	private readonly int _userId = userId;

	public override IQueryable<Trade> Apply(IQueryable<Trade> query)
	{
		query = query.Where(x => x.UserId == _userId);

		if (!string.IsNullOrWhiteSpace(Filter.SearchText))
		{
			var searchText = Filter.SearchText;

			if (double.TryParse(searchText, out var textPriceDouble) &&
				decimal.TryParse(searchText, out var textPriceDecemal))
			{
				query = query.Where(x =>
					x.ClosePrice != null && x.ClosePrice == textPriceDouble ||
					x.OpenPrice == textPriceDouble ||
					x.TotalPrice == textPriceDecemal);
			}
			else if (DateTime.TryParse(searchText, out var date))
			{
				query = query.Where(x =>
					(x.OpenedAt.Date == date.Date) ||
					(x.ClosedAt != null && x.ClosedAt.Value.Date == date.Date));
			}
			else
			{
				query = query.Where(x =>
					(x.Instrument != null && x.Instrument.Symbol.Contains(searchText)) ||
					x.Instrument != null && x.Instrument.Description != null && x.Instrument.Description.Contains(searchText));
			}
		}

		return query;
	}
}
