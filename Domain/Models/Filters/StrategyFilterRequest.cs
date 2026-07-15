using Domain.Models.Pagination;

namespace Domain.Models.Filters;

public record StrategyFilterRequest
{
	public bool? IsActive { get; init; }
}
