using Application.Common.Queries;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Extensions;

public static class QueryableExtensions
{
	public static async Task<PageResult<T>> ToPagedAsync<T>(
		this IQueryable<T> source,
		PageOptions page,
		CancellationToken ct = default)
	{
		var totalCount = await source.CountAsync(ct);
		if (totalCount == 0)
			return new PageResult<T>([], 0, page.Page, page.PageSize);

		var items = await source
			.Skip((page.Page - 1) * page.PageSize)
			.Take(page.PageSize)
			.ToListAsync(ct);

		return new PageResult<T>(items, totalCount, page.Page, page.PageSize);
	}
}
