using Domain.Strategies.Enums;
using System.ComponentModel;

namespace Application.Common.Models.Sort;

public record StrategySortRequest() : BaseSortRequest<StrategySortField>
{
	[DefaultValue(StrategySortField.NameAsc)]
	protected override List<StrategySortField> DefaultSortBy => [StrategySortField.NameAsc];
}
