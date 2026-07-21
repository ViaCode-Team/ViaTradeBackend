using Domain.Trades.Enums;

namespace Application.Trades.Models;

public record TradeDto(
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
