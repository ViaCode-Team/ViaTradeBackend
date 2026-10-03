using Ardalis.Specification;
using ViaTrade.Application.Common.Models;
using ViaTrade.Application.Common.Specifications;
using ViaTrade.Application.Trades.Models;
using ViaTrade.Domain.Entities;
using ViaTrade.Domain.Enums;

namespace ViaTrade.Application.Trades.Specifications;

public class TradesPageSpecification : PageSpecification<Trade>
{
	public TradesPageSpecification(
		int userId,
		TradeFilter tradeFilter,
		TradeSearch tradeSearch,
		PageOptions pageOptions
	)
		: base(pageOptions)
	{
		Query.Where(x => x.UserId == userId);

		ApplyFilter(tradeFilter);

		ApplySearch(tradeSearch);

		AddOrderByAscending(entity => entity.Id);
	}

	private void ApplyFilter(TradeFilter tradeFilter)
	{
		if (tradeFilter.Signal.HasValue)
			Query.Where(x => x.Signal == tradeFilter.Signal.Value);

		if (tradeFilter.Status == TradeStatus.Open)
			Query.Where(x => x.ClosedAt == null);
		else if (tradeFilter.Status == TradeStatus.Closed)
			Query.Where(x => x.ClosedAt != null);

		if (!string.IsNullOrWhiteSpace(tradeFilter.TradeTypeName))
			Query.Where(x => x.TradeType!.Name == tradeFilter.TradeTypeName);

		if (tradeFilter.StartDate.HasValue)
			Query.Where(x => x.OpenedAt >= tradeFilter.StartDate.Value);

		if (tradeFilter.EndDate.HasValue)
			Query.Where(x => x.OpenedAt <= tradeFilter.EndDate.Value);
	}

	private void ApplySearch(TradeSearch tradeSearch)
	{
		var searchText = tradeSearch.GetNormalizedSearchText();
		if (searchText == null)
			return;

		var isDouble = double.TryParse(searchText, out var textPriceDouble);
		var isDecimal = decimal.TryParse(searchText, out var textPriceDecimal);
		var isDate = DateTime.TryParse(searchText, out var date);

		DateTime nextDay = default;
		if (isDate)
			nextDay = date.Date.AddDays(1);

		Query.Where(x =>
			(isDouble && (x.ClosePrice == textPriceDouble || x.OpenPrice == textPriceDouble))
			|| (isDecimal && x.TotalPrice == textPriceDecimal)
			|| (
				isDate
				&& (
					(x.OpenedAt >= date.Date && x.OpenedAt < nextDay)
					|| (x.ClosedAt >= date.Date && x.ClosedAt < nextDay)
				)
			)
			|| x.Instrument!.Symbol.Contains(searchText)
		);
	}
}
