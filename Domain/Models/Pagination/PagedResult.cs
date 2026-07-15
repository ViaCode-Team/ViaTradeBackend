using System.Text.Json.Serialization;

namespace Domain.Models.Pagination;

public sealed record PagedResult<T>(
	IReadOnlyList<T> Items,
	int TotalCount,
	[property: JsonIgnore] int PageNumber,
	[property: JsonIgnore] int PageSize)
{
	public int TotalPages => CalculateTotalPages();

	private int CalculateTotalPages()
	{
		if (TotalCount == 0)
			return 0;

		int totalPages = TotalCount / PageSize;
		int remainder = TotalCount % PageSize;

		if (remainder > 0)
			totalPages++;

		return totalPages;
	}

	public PagedResult<TResult> Map<TResult>(Func<T, TResult> mapFunc)
	{
		return new PagedResult<TResult>(Items.Select(mapFunc).ToList(), TotalCount, PageNumber, PageSize);
	}
}
