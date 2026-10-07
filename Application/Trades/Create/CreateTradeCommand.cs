using ViaTrade.Application.Common.Abstractions;
using ViaTrade.Application.Trades.Common;
using ViaTrade.Domain.Enums;

namespace ViaTrade.Application.Trades.Create;

public sealed record CreateTradeCommand(
	int InstrumentId,
	int TradeTypeId,
	DateTime OpenedAt,
	DateTime? ClosedAt,
	double OpenPrice,
	double? ClosePrice,
	TradeSignal Signal,
	int Quantity
) : ICommand<TradeResult>;
