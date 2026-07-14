using System.Text.Json.Serialization;

namespace Domain.Models.Pagination;

public sealed record PagedResult<T>(
	IReadOnlyList<T> Items,
	int TotalCount,
	[property: JsonIgnore] int PageNumber,
	[property: JsonIgnore] int PageSize)
{
	public int TotalPages => CalculateTotalPages(TotalCount, PageSize);

	private static int CalculateTotalPages(int totalCount, int pageSize)
	{
		if (totalCount == 0)
			return 0;

		return (int)Math.Ceiling(totalCount / (double)pageSize);
	}
}
