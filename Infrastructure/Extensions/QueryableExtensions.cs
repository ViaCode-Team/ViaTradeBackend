using Domain.Models.Pagination;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Extensions;

public static class QueryableExtensions
{
	public static async Task<PagedResult<T>> ToPagedResultAsync<T>(
		this IQueryable<T> source,
		PaginationRequest paginationRequest,
		CancellationToken ct = default)
	{
		var totalCount = await source.CountAsync(ct);

		if (!source.Expression.ToString().Contains("OrderBy"))
		{
			var idProperty = typeof(T).GetProperty("Id");
			if (idProperty != null)
			{
				source = source.OrderBy(e => EF.Property<object>(e, "Id"));
			}
		}

		var items = await source
			.Skip((paginationRequest.PageNumber - 1) * paginationRequest.PageSize)
			.Take(paginationRequest.PageSize)
			.ToListAsync(ct);

		return new PagedResult<T>(items, totalCount, paginationRequest.PageNumber, paginationRequest.PageSize);
	}
}
