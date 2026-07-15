using Domain.Entities.DataBase;
using Domain.Enums;
using Domain.Models.Pagination;

namespace Domain.Models.Filters;

public record TradeFilterRequest
{
	public TradeSignal? Signal { get; init; }
	public TradeStatus? Status { get; init; }
	public string? TradeTypeName { get; init; }
	public DateTime? StartDate { get; init; }
	public DateTime? EndDate { get; init; }
}
