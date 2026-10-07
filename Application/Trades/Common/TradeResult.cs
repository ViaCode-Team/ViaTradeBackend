using System.Linq.Expressions;
using ViaTrade.Application.Common.Models;
using ViaTrade.Domain.Entities;
using ViaTrade.Domain.Enums;

namespace ViaTrade.Application.Trades.Common;

public record TradeResult(
	int Id,
	DateTime OpenedAt,
	DateTime? ClosedAt,
	double OpenPrice,
	double? ClosePrice,
	double? NetIncome,
	int Quantity,
	decimal TotalPrice,
	TradeSignal Signal,
	int TradeTypeId,
	InstrumentBriefResult? Instrument,
	int UserId
)
{
	public static Expression<Func<Trade, TradeResult>> Projection { get; } =
		trade => new TradeResult(
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
			new InstrumentBriefResult(trade.Instrument!.Id, trade.Instrument.Ticker, trade.Instrument.Description),
			trade.UserId
		);
}
