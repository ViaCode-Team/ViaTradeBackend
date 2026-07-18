using Domain.Trades.Enums;
using Domain.Trades.Entities;
using Domain.Entities.DataBase;
using Domain.Enums;

namespace Domain.Models.Filters;

public record TradeFilterRequest(
	TradeSignal? Signal,
	TradeStatus? Status,
	string? TradeTypeName,
	DateTime? StartDate,
	DateTime? EndDate
);
