using Application.Common.Queries;

namespace Application.Strategies.Queries;

public record StrategySort() : Sort<StrategySortField>
{
	protected override List<StrategySortField> DefaultSortBy => [StrategySortField.NameAsc];
}
