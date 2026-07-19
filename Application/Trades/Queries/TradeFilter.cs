
using Domain.Trades.Enums;

namespace Application.Trades.Queries;

public record TradeFilter(
	TradeSignal? Signal,
	TradeStatus? Status,
	string? TradeTypeName,
	DateTime? StartDate,
	DateTime? EndDate
);
