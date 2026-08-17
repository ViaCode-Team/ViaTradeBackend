using ViaTrade.Application.Common.Models;
using ViaTrade.Application.Common.Specifications;
using ViaTrade.Domain.Entities;

namespace ViaTrade.Application.Trades.Specifications;

public sealed class TradeSearchSpecification(int userId, SearchFilter filter)
	: SearchSpecification<Trade, SearchFilter>(filter)
{
	private readonly int _userId = userId;

	public override IQueryable<Trade> Apply(IQueryable<Trade> query)
	{
		query = query.Where(x => x.UserId == _userId);

		if (string.IsNullOrWhiteSpace(Filter.SearchText))
			return query;

		var searchText = Filter.SearchText;
		var isDouble = double.TryParse(searchText, out var textPriceDouble);
		var isDecimal = decimal.TryParse(searchText, out var textPriceDecimal);
		var isDate = DateTime.TryParse(searchText, out var date);

		query = query.Where(x =>
			(
				isDouble
				&& isDecimal
				&& (
					(x.ClosePrice != null && x.ClosePrice == textPriceDouble)
					|| x.OpenPrice == textPriceDouble
					|| x.TotalPrice == textPriceDecimal
				)
			)
			|| (isDate && (x.OpenedAt.Date == date.Date || (x.ClosedAt != null && x.ClosedAt.Value.Date == date.Date)))
			|| (
				x.Instrument != null
				&& (
					x.Instrument.Symbol.Contains(searchText)
					|| (x.Instrument.Description != null && x.Instrument.Description.Contains(searchText))
				)
			)
		);

		return query;
	}
}
