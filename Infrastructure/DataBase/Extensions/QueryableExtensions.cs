using Microsoft.EntityFrameworkCore;
using ViaTrade.Application.Common.Models;

namespace ViaTrade.Infrastructure.DataBase.Extensions;

public static class QueryableExtensions
{
	public static Task<PageResult<T>> ToPagedAsync<T>(
		this IQueryable<T> source,
		PageOptions pageOptions,
		CancellationToken ct = default
	)
	{
		return source.ToMultiPageAsync(pageOptions, ct);
	}

	public static Task<PageResult<T>> ToPagedAsync<T>(
		this IQueryable<T> source,
		PageOptions pageOptions,
		bool isUniqueRequest,
		CancellationToken ct = default
	)
	{
		if (isUniqueRequest)
			return source.ToSinglePageAsync(pageOptions, ct);

		return source.ToMultiPageAsync(pageOptions, ct);
	}

	public static async Task<PageResult<T>> ToMultiPageAsync<T>(
		this IQueryable<T> source,
		PageOptions pageOptions,
		CancellationToken ct = default
	)
	{
		if (pageOptions.Page == 1)
		{
			var items = await source.Take(pageOptions.PageSize + 1).ToListAsync(ct);

			if (items.Count <= pageOptions.PageSize)
				return new PageResult<T>(items, items.Count, pageOptions.Page, pageOptions.PageSize);

			var totalCount = await source.CountAsync(ct);
			items.RemoveAt(items.Count - 1);

			return new PageResult<T>(items, totalCount, pageOptions.Page, pageOptions.PageSize);
		}

		var total = await source.CountAsync(ct);
		return await source.ToMultiPageAsync(pageOptions, total, ct);
	}

	public static async Task<PageResult<T>> ToMultiPageAsync<T>(
		this IQueryable<T> source,
		PageOptions pageOptions,
		int totalCount,
		CancellationToken ct = default
	)
	{
		if (totalCount == 0)
			return new PageResult<T>([], 0, pageOptions.Page, pageOptions.PageSize);

		var skip = checked((pageOptions.Page - 1) * pageOptions.PageSize);

		var items = await source.Skip(skip).Take(pageOptions.PageSize).ToListAsync(ct);

		return new PageResult<T>(items, totalCount, pageOptions.Page, pageOptions.PageSize);
	}

	public static async Task<PageResult<T>> ToSinglePageAsync<T>(
		this IQueryable<T> source,
		PageOptions pageOptions,
		CancellationToken ct = default
	)
	{
		if (pageOptions.Page > 1)
			return new PageResult<T>([], 0, pageOptions.Page, pageOptions.PageSize);

		var item = await source.FirstOrDefaultAsync(ct);

		T[] items = [];
		if (item != null)
			items = [item];

		return new PageResult<T>(items, items.Length, pageOptions.Page, pageOptions.PageSize);
	}
}
