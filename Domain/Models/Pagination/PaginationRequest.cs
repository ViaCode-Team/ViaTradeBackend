using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Domain.Models.Pagination;

public record PaginationRequest
{
	public const int MaxPageSize = 100;

	[DefaultValue(1)]
	[Range(1, int.MaxValue)]
	public int Page { get; init; } = 1;

	[DefaultValue(20)]
	[Range(1, MaxPageSize)]
	public int PageSize { get; init; } = 20;
}
