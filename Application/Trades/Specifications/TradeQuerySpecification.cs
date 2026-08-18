using ViaTrade.Application.Common.Specifications;
using ViaTrade.Application.Trades.Models;
using ViaTrade.Domain.Entities;
using ViaTrade.Domain.Enums;

namespace ViaTrade.Application.Trades.Specifications;

public class TradeQuerySpecification : BaseQuerySpecification<Trade>
{
	public TradeQuerySpecification(int userId, TradeFilter tradeFilter, TradeSearch tradeSearch)
	{
		AddCriteria(x => x.UserId == userId);

		var isOpenStatus = tradeFilter.Status == TradeStatus.Open;
		var hasTradeTypeName = !string.IsNullOrWhiteSpace(tradeFilter.TradeTypeName);

		ApplyFilter(tradeFilter, isOpenStatus, hasTradeTypeName);

		ApplySearch(tradeSearch, isOpenStatus);
	}

	private void ApplyFilter(TradeFilter tradeFilter, bool isOpenStatus, bool hasTradeTypeName)
	{
		if (tradeFilter.Signal.HasValue)
			AddCriteria(x => x.Signal == tradeFilter.Signal.Value);

		if (isOpenStatus)
			AddCriteria(x => x.ClosedAt == null);
		else if (tradeFilter.Status == TradeStatus.Closed)
			AddCriteria(x => x.ClosedAt != null);

		if (hasTradeTypeName)
			AddCriteria(x => x.TradeType != null && x.TradeType.Name == tradeFilter.TradeTypeName);

		if (tradeFilter.StartDate.HasValue)
			AddCriteria(x => x.OpenedAt >= tradeFilter.StartDate.Value);

		if (tradeFilter.EndDate.HasValue)
			AddCriteria(x => x.OpenedAt <= tradeFilter.EndDate.Value);
	}

	private void ApplySearch(TradeSearch tradeSearch, bool excludeClosedAt)
	{
		var searchText = tradeSearch.GetNormalizedSearchText();
		if (searchText == null)
			return;

		var isDouble = double.TryParse(searchText, out var textPriceDouble);
		var isDecimal = decimal.TryParse(searchText, out var textPriceDecimal);
		var isDate = DateTime.TryParse(searchText, out var date);

		AddCriteria(x =>
			(isDouble && ((x.ClosePrice != null && x.ClosePrice == textPriceDouble) || x.OpenPrice == textPriceDouble))
			|| isDecimal && x.TotalPrice == textPriceDecimal
			|| (
				isDate
				&& (
					x.OpenedAt.Date == date.Date
					|| (!excludeClosedAt && x.ClosedAt != null && x.ClosedAt.Value.Date == date.Date)
				)
			)
			|| (
				x.Instrument != null
				&& (
					(x.Instrument.Symbol.Contains(searchText))
					|| (x.Instrument.Description != null && x.Instrument.Description.Contains(searchText))
				)
			)
		);
	}
}
