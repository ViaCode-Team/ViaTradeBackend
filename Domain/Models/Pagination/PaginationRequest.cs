using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Domain.Models.Pagination;

public record PaginationRequest(
	[DefaultValue(1), Range(1, int.MaxValue)] int Page = 1,
	[DefaultValue(20), Range(1, 100)] int PageSize = 20
)
{
	public const int MaxPageSize = 100;
}
