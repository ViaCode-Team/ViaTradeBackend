using Domain.Reminds.Enums;
using System.ComponentModel;

namespace Application.Common.Models.Sort;

public record ReminderSortRequest() : BaseSortRequest<RemindSortField>
{
	[DefaultValue(RemindSortField.DateTimeDesc)]
	protected override List<RemindSortField> DefaultSortBy => [RemindSortField.DateTimeDesc];
}
