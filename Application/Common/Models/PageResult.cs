namespace ViaTrade.Application.Common.Models;

public sealed record PageResult<T>
{
	public IReadOnlyList<T> Items { get; }

	public int TotalCount { get; }

	public int Page { get; }

	public int PageSize { get; }

	public int TotalPages { get; }

	public PageResult(IReadOnlyList<T> items, int totalCount, int pageNumber, int pageSize)
	{
		Items = items;
		TotalCount = totalCount;
		Page = pageNumber;
		PageSize = pageSize;
		TotalPages = CalculateTotalPages(totalCount, pageSize);
	}

	public static PageResult<T> FromList(IReadOnlyList<T> items, int pageNumber, int pageSize)
	{
		ArgumentNullException.ThrowIfNull(items);
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pageNumber);
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pageSize);

		var offset = checked((pageNumber - 1) * pageSize);
		if (offset >= items.Count)
			return new PageResult<T>([], items.Count, pageNumber, pageSize);

		var pageItems = items.Skip(offset).Take(pageSize).ToList();
		return new PageResult<T>(pageItems, items.Count, pageNumber, pageSize);
	}

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
