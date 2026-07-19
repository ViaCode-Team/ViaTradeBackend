using Domain.Reminds.Enums;
namespace Application.Common.Models.Sort;

public record RemindSortRequest() : BaseSortRequest<RemindSortField>
{
	protected override List<RemindSortField> DefaultSortBy => [RemindSortField.DateTimeDesc];
}
