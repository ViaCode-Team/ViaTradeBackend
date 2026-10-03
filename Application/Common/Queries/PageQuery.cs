using System.Linq.Expressions;
using ViaTrade.Application.Common.Interfaces.Repositories;
using ViaTrade.Application.Common.Models;
using ViaTrade.Application.Common.Specifications;

namespace ViaTrade.Application.Common.Queries;

public static class PageQuery
{
	public static Task<PageResult<T>> ExecuteAsync<T>(
		IReadRepository<T> repository,
		PageSpecification<T> specification,
		CancellationToken ct
	)
		where T : class
	{
		var pageSpecification = specification.ForPage(true);
		return ExecuteCoreAsync(repository, specification, () => repository.ListAsync(pageSpecification, ct), ct);
	}

	public static Task<PageResult<TResult>> ExecuteAsync<T, TResult>(
		IReadRepository<T> repository,
		PageSpecification<T> specification,
		Expression<Func<T, TResult>> selector,
		CancellationToken ct
	)
		where T : class
	{
		var pageSpecification = specification.ForPage(selector, true);
		return ExecuteCoreAsync(repository, specification, () => repository.ListAsync(pageSpecification, ct), ct);
	}

	public static PageResult<T> FromList<T>(IReadOnlyList<T> items, PageSpecification<T> specification)
	{
		if (specification.Offset >= items.Count)
			return new PageResult<T>([], items.Count, specification.Page, specification.PageSize);

		var pageItems = items.Skip(specification.Offset).Take(specification.PageSize).ToList();
		return new PageResult<T>(pageItems, items.Count, specification.Page, specification.PageSize);
	}

	private static async Task<PageResult<TResult>> ExecuteCoreAsync<T, TResult>(
		IReadRepository<T> repository,
		PageSpecification<T> specification,
		Func<Task<List<TResult>>> listAsync,
		CancellationToken ct
	)
		where T : class
	{
		ct.ThrowIfCancellationRequested();
		if (specification.Page == 1)
		{
			var items = await listAsync();
			if (items.Count <= specification.PageSize)
				return new PageResult<TResult>(items, items.Count, specification.Page, specification.PageSize);

			var totalCount = await repository.CountAsync(specification, ct);
			items.RemoveAt(items.Count - 1);
			return new PageResult<TResult>(items, totalCount, specification.Page, specification.PageSize);
		}

		var total = await repository.CountAsync(specification, ct);
		if (specification.Offset >= total)
			return new PageResult<TResult>([], total, specification.Page, specification.PageSize);

		var pageItems = await listAsync();
		return new PageResult<TResult>(pageItems, total, specification.Page, specification.PageSize);
	}
}
