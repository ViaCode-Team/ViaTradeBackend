using ViaTrade.Application.Common.Models;

namespace ViaTrade.Application.Strategies.Common;

public record StrategySort() : Sort<StrategySortField>
{
	protected override List<StrategySortField> DefaultSortBy => [StrategySortField.NameAsc];
}
