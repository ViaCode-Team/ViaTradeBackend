using Microsoft.EntityFrameworkCore;
using ViaTrade.Application.Common.Interfaces;
using ViaTrade.Domain.Entities;

namespace ViaTrade.Infrastructure.Extensions;

public static class SpecificationEvaluator
{
	public static IQueryable<TEntity> GetQuery<TEntity>(
		IQueryable<TEntity> query,
		IQuerySpecification<TEntity> specification
	)
		where TEntity : BaseEntity<int>
	{
		query = ApplyCriteria(query, specification);
		query = ApplyIncludes(query, specification);
		query = ApplySorting(query, specification);

		if (specification.IsSplitQuery)
			query = query.AsSplitQuery();

		return query;
	}

	public static IQueryable<TEntity> GetQueryForPagination<TEntity>(
		IQueryable<TEntity> query,
		IQuerySpecification<TEntity> specification
	)
		where TEntity : BaseEntity<int>
	{
		var result = GetQuery(query, specification);

		if (specification.SortExpressions.Count == 0)
			result = result.OrderBy(entity => entity.Id);

		return result;
	}

	private static IQueryable<TEntity> ApplyCriteria<TEntity>(
		IQueryable<TEntity> query,
		IQuerySpecification<TEntity> specification
	)
		where TEntity : BaseEntity<int>
	{
		foreach (var criterion in specification.Criteria)
			query = query.Where(criterion);

		return query;
	}

	private static IQueryable<TEntity> ApplyIncludes<TEntity>(
		IQueryable<TEntity> query,
		IQuerySpecification<TEntity> specification
	)
		where TEntity : BaseEntity<int>
	{
		foreach (var include in specification.Includes)
			query = query.Include(include);

		return query;
	}

	private static IQueryable<TEntity> ApplySorting<TEntity>(
		IQueryable<TEntity> query,
		IQuerySpecification<TEntity> specification
	)
		where TEntity : BaseEntity<int>
	{
		if (specification.SortExpressions.Count <= 0)
			return query;

		IOrderedQueryable<TEntity> orderedQuery;

		var (FirstKeySelector, FirstIsDescending) = specification.SortExpressions[0];
		if (FirstIsDescending)
			orderedQuery = query.OrderByDescending(FirstKeySelector);
		else
			orderedQuery = query.OrderBy(FirstKeySelector);

		var sortedQuery = specification
			.SortExpressions.Skip(1)
			.Aggregate(
				orderedQuery,
				(current, sortExpression) =>
				{
					if (sortExpression.IsDescending)
						return current.ThenByDescending(sortExpression.KeySelector);

					return current.ThenBy(sortExpression.KeySelector);
				}
			);

		return sortedQuery.ThenBy(entity => entity.Id);
	}
}
