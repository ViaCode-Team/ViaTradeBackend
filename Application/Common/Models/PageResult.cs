namespace Application.Common.Models;

public sealed class PageResult<T>(IReadOnlyList<T> items, int totalCount, int pageNumber, int pageSize)
{
	public IReadOnlyList<T> Items { get; } = items;

	public int TotalCount { get; } = totalCount;

	public int Page { get; } = pageNumber;

	public int PageSize { get; } = pageSize;

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

	public PageResult<TResult> Map<TResult>(Func<T, TResult> mapFunc)
	{
		ArgumentNullException.ThrowIfNull(mapFunc);

		return new PageResult<TResult>(Items.Select(mapFunc).ToList(), TotalCount, Page, PageSize);
	}
}
