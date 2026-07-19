using Application.Common.Models.Pagination;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Extensions;

public static class QueryableExtensions
{
	public static async Task<PagedResult<T>> ToPagedAsync<T>(
		this IQueryable<T> source,
		PaginationRequest paginationRequest,
		CancellationToken ct = default)
	{
		paginationRequest ??= new PaginationRequest();

		var totalCount = await source.CountAsync(ct);
		if (totalCount == 0)
			return new PagedResult<T>([], 0, paginationRequest.Page, paginationRequest.PageSize);

		var items = await source
			.Skip((paginationRequest.Page - 1) * paginationRequest.PageSize)
			.Take(paginationRequest.PageSize)
			.ToListAsync(ct);

		return new PagedResult<T>(items, totalCount, paginationRequest.Page, paginationRequest.PageSize);
	}
}
