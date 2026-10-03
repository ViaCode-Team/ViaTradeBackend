using System.Linq.Expressions;
using Ardalis.Specification;
using ViaTrade.Application.Common.Models;

namespace ViaTrade.Application.Common.Specifications;

public class PageSpecification<T> : OrderedSpecification<T>
{
	public int Page { get; }
	public int PageSize { get; }
	public int Offset { get; }

	public PageSpecification(PageOptions pageOptions)
	{
		Page = pageOptions.Page;
		PageSize = pageOptions.PageSize;
		Offset = checked((Page - 1) * PageSize);
	}

	public Specification<T, TResult> Project<TResult>(Expression<Func<T, TResult>> selector)
	{
		var projection = new Specification<T, TResult>();
		projection.Query.Select(selector);
		return this.WithProjectionOf(projection);
	}

	public Specification<T, T> ForPage(bool includeNextItem = false)
	{
		return ForPage(entity => entity, includeNextItem);
	}

	public Specification<T, TResult> ForPage<TResult>(
		Expression<Func<T, TResult>> selector,
		bool includeNextItem = false
	)
	{
		var specification = Project(selector);
		var take = PageSize;
		if (includeNextItem && Page == 1)
			take++;

		specification.Query.Skip(Offset).Take(take);
		return specification;
	}
}
