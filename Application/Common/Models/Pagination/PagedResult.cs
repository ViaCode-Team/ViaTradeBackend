namespace Application.Common.Models.Pagination;

public sealed class PagedResult<T>(
	IReadOnlyList<T> items, int totalCount, int pageNumber, int pageSize)
{
	public IReadOnlyList<T> Items { get; } = items;

	public int TotalCount { get; } = totalCount;

	public int TotalPages { get; } = CalculateTotalPages(totalCount, pageSize);

	private static int CalculateTotalPages(int totalCount, int pageSize)
	{
		if (pageSize < 1)
			throw new ArgumentException("PageSize must be a positive integer.", nameof(pageSize));

		if (totalCount == 0)
			return 0;

		int totalPages = Math.DivRem(totalCount, pageSize, out int remainder);
		if (remainder > 0)
			totalPages++;

		return totalPages;
	}

	public PagedResult<TResult> Map<TResult>(Func<T, TResult> mapFunc)
	{
		if (mapFunc == null)
			throw new KeyNotFoundException(nameof(mapFunc));

		return new PagedResult<TResult>(
			Items.Select(mapFunc).ToList(),
			TotalCount,
			pageNumber,
			pageSize);
	}
}
