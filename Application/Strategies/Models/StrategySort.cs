using Application.Common.Models;

namespace Application.Strategies.Models;

public record StrategySort() : Sort<StrategySortField>
{
	protected override List<StrategySortField> DefaultSortBy => [StrategySortField.NameAsc];
}
