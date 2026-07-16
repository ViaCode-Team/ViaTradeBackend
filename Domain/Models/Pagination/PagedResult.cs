namespace Domain.Models.Pagination;

public sealed class PagedResult<T>(
	IReadOnlyList<T> items,
	int totalCount,
	int pageNumber,
	int pageSize)
{
	private readonly int _pageNumber = pageNumber;
	private readonly int _pageSize = pageSize;

	public IReadOnlyList<T> Items { get; } = items;

	public int TotalCount { get; } = totalCount;

	public int TotalPages { get; } = CalculateTotalPages(totalCount, pageSize);

	private static int CalculateTotalPages(int totalCount, int pageSize)
	{
		ArgumentOutOfRangeException.ThrowIfLessThan(pageSize, 1);

		if (totalCount == 0)
			return 0;

		int totalPages = Math.DivRem(totalCount, pageSize, out int remainder);
		if (remainder > 0)
			totalPages++;

		return totalPages;
	}

	public PagedResult<TResult> Map<TResult>(Func<T, TResult> mapFunc)
	{
		ArgumentNullException.ThrowIfNull(mapFunc);

		return new PagedResult<TResult>(
			Items.Select(mapFunc).ToList(),
			TotalCount,
			_pageNumber,
			_pageSize);
	}
}
