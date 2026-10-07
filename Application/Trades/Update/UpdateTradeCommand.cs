using Mediator;
using ViaTrade.Domain.Enums;

namespace ViaTrade.Application.Trades.Update;

public sealed record UpdateTradeCommand(
	int TradeId,
	int InstrumentId,
	int TradeTypeId,
	DateTime OpenedAt,
	DateTime? ClosedAt,
	double OpenPrice,
	double? ClosePrice,
	TradeSignal Signal,
	int Quantity
) : ICommand;
