using Application.Common.Interfaces;
using Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Extensions;

public static class SpecificationEvaluator
{
	public static IQueryable<TEntity> GetQuery<TEntity>(
		IQueryable<TEntity> query,
		IQuerySpecification<TEntity> specification) where TEntity : DomainEntity<int>
	{
		query = ApplyCriteria(query, specification);
		query = ApplyIncludes(query, specification);
		query = ApplySorting(query, specification);

		if (specification.IsSplitQuery)
			query = query.AsSplitQuery();

		return query;
	}

	private static IQueryable<TEntity> ApplyCriteria<TEntity>(
		IQueryable<TEntity> query,
		IQuerySpecification<TEntity> specification) where TEntity : DomainEntity<int>
	{
		foreach (var criterion in specification.Criteria)
			query = query.Where(criterion);

		return query;
	}

	private static IQueryable<TEntity> ApplyIncludes<TEntity>(
		IQueryable<TEntity> query,
		IQuerySpecification<TEntity> specification) where TEntity : DomainEntity<int>
	{
		foreach (var include in specification.Includes)
			query = query.Include(include);

		return query;
	}

	private static IQueryable<TEntity> ApplySorting<TEntity>(
		IQueryable<TEntity> query,
		IQuerySpecification<TEntity> specification) where TEntity : DomainEntity<int>
	{
		if (specification.SortExpressions.Count == 0)
			return query;

		var firstSort = specification.SortExpressions[0];
		IOrderedQueryable<TEntity> orderedQuery;

		if (firstSort.IsDescending)
			orderedQuery = query.OrderByDescending(firstSort.KeySelector);
		else
			orderedQuery = query.OrderBy(firstSort.KeySelector);

		return specification.SortExpressions
			.Skip(1)
			.Aggregate(orderedQuery,
				(current, sort) =>
				{
					if (sort.IsDescending)
						return current.ThenByDescending(sort.KeySelector);

					return current.ThenBy(sort.KeySelector);
				});
	}
}
