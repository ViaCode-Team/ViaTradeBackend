using System.ComponentModel.DataAnnotations;
using Domain.Enums;

namespace ViaTradeBackend.Contracts.Trades;

public record CreateTradeRequest(
	DateTime OpenedAt,
	DateTime? ClosedAt,
	double EntryPrice,
	double? ExitPrice,
	TradeSignal Signal,
	[Range(1, int.MaxValue)] int Quantity,
	int TradeTypeId,
	int InstrumentId
);
