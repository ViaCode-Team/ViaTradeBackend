using System.ComponentModel.DataAnnotations;
using Domain.Trades.Enums;

namespace ViaTradeBackend.Contracts.Trades;

public record UpdateTradeRequest(
	DateTime DateOpen,
	DateTime? DateClose,
	double TradeOpen,
	double? TradeClose,
	TradeSignal TradeSignal,
	[Range(1, int.MaxValue)] int Count,
	int TradeTypeId,
	int InstrumentId
);
