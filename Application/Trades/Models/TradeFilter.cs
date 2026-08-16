using ViaTrade.Domain.Enums;

namespace ViaTrade.Application.Trades.Models;

public record TradeFilter(
	TradeSignal? Signal,
	TradeStatus? Status,
	string? TradeTypeName,
	DateTime? StartDate,
	DateTime? EndDate
);
