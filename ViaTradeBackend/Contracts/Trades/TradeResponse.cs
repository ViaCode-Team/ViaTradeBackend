using Domain.Trades.Enums;

namespace ViaTradeBackend.Contracts.Trades;

public record TradeResponse(
	int Id,
	DateTime DateOpen,
	DateTime? DateClose,
	double TradeOpen,
	double? TradeClose,
	double? NetIncome,
	int Count,
	decimal Price,
	TradeSignal TradeSignal,
	int TradeTypeId,
	int TradeCodeId,
	int UserId
);
