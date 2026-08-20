using EntityFrameworkCore.Projectables;
using ViaTrade.Application.Trades.Models;
using ViaTrade.Domain.Entities;
using ViaTrade.Domain.Enums;

namespace ViaTrade.Infrastructure.DataBase.Extensions;

public static class TradeProjectableExtensions
{
	[Projectable]
	public static TradeProjectionDto ToTradeProjectionDto(this Trade trade) =>
		new(
			trade.Id,
			trade.OpenedAt,
			trade.ClosedAt,
			trade.OpenPrice,
			trade.ClosePrice,
			trade.NetIncome,
			trade.Quantity,
			trade.TotalPrice,
			trade.Signal,
			trade.TradeTypeId,
			new(trade.Instrument!.Id, trade.Instrument.Symbol, trade.Instrument.Description),
			trade.UserId
		);

	[Projectable]
	public static bool IsClosedTrade(this Trade trade) => trade.ClosedAt.HasValue && trade.ClosePrice.HasValue;

	[Projectable]
	public static bool IsProfitCalculable(this Trade trade) => trade.OpenPrice != 0 && trade.Signal != TradeSignal.HOLD;

	[Projectable]
	public static ProfitChartAggregateRow ToProfitChartAggregateRow(
		this IEnumerable<Trade> group,
		int? year,
		int? month,
		int? day,
		int? week
	) =>
		new(
			year,
			month,
			day,
			week,
			Math.Round(group.Sum(trade => trade.NetIncome!.Value), 2),
			Math.Round(group.Where(trade => trade.Signal == TradeSignal.BUY).Sum(trade => trade.NetIncome!.Value), 2),
			Math.Round(group.Where(trade => trade.Signal == TradeSignal.SELL).Sum(trade => trade.NetIncome!.Value), 2)
		);
}
