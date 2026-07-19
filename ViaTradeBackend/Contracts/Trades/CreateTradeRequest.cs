using Domain.Trades.Enums;
using System.ComponentModel.DataAnnotations;

namespace ViaTradeBackend.Contracts.Trades;

public record CreateTradeRequest(
	DateTime DateOpen,
	DateTime? DateClose,
	double TradeOpen,
	double? TradeClose,
	TradeSignal TradeSignal,
	[Range(0, int.MaxValue)] int Count,
	int TradeTypeId,
	int TradeCodeId
);

