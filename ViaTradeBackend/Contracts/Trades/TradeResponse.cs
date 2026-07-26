using Domain.Enums;
using ViaTradeBackend.Contracts.Instruments;

namespace ViaTradeBackend.Contracts.Trades;

public record TradeResponse(
	int Id,
	DateTime OpenedAt,
	DateTime? ClosedAt,
	double EntryPrice,
	double? ExitPrice,
	double? NetIncome,
	int Quantity,
	decimal TotalPrice,
	TradeSignal Signal,
	int TradeTypeId,
	InstrumentBriefResponse? Instrument,
	int UserId
);
