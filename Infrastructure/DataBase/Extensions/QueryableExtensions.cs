using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using ViaTrade.Application.Common.Interfaces;
using ViaTrade.Application.Common.Models;
using ViaTrade.Domain.Entities;

namespace ViaTrade.Infrastructure.DataBase.Extensions;

public static class QueryableExtensions
{
	public static async Task<PageResult<T>> ToPagedAsync<T>(
		this IQueryable<T> source,
		PageOptions pageOptions,
		CancellationToken ct
	)
	{
		var totalCount = await source.CountAsync(ct);

		return await source.ToPagedAsync(pageOptions, totalCount, ct);
	}

	public static async Task<PageResult<T>> ToPagedAsync<T>(
		this IQueryable<T> source,
		PageOptions pageOptions,
		int totalCount,
		CancellationToken ct
	)
	{
		if (totalCount == 0)
			return new PageResult<T>([], 0, pageOptions.Page, pageOptions.PageSize);

		var items = await source
			.Skip((pageOptions.Page - 1) * pageOptions.PageSize)
			.Take(pageOptions.PageSize)
			.ToListAsync(ct);

		return new PageResult<T>(
			items,
			totalCount,
			pageOptions.Page,
			pageOptions.PageSize
		);
	}

}
