using Domain.Trades.Enums;

namespace Application.Common.Models.Filters;

public record TradeFilterRequest(
	TradeSignal? Signal,
	TradeStatus? Status,
	string? TradeTypeName,
	DateTime? StartDate,
	DateTime? EndDate
);
