using Application.Common.Models;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Extensions;

public static class QueryableExtensions
{
	public static async Task<PageResult<T>> ToPagedAsync<T>(
		this IQueryable<T> source,
		PageOptions pageOptions,
		CancellationToken ct
	)
	{
		var totalCount = await source.CountAsync(ct);
		if (totalCount == 0)
			return new PageResult<T>([], 0, pageOptions.Page, pageOptions.PageSize);

		var items = await source
			.Skip((pageOptions.Page - 1) * pageOptions.PageSize)
			.Take(pageOptions.PageSize)
			.ToListAsync(ct);

		return new PageResult<T>(items, totalCount, pageOptions.Page, pageOptions.PageSize);
	}
}
