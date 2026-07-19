using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Application.Common.Models.Pagination;

public class PaginationRequest
{
	public const int MaxPageSize = 100;

	[DefaultValue(1), Range(1, int.MaxValue)]
	public int Page { get; set; } = 1;

	[DefaultValue(20), Range(1, MaxPageSize)]
	public int PageSize { get; set; } = 20;
}
