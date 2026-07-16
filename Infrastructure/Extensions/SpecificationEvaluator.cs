using Domain.Entities.DataBase;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Extensions;

public static class SpecificationEvaluator
{
	public static IQueryable<TEntity> GetQuery<TEntity>(
		IQueryable<TEntity> query,
		ISpecification<TEntity> specification) where TEntity : BaseEntity
	{
		foreach (var criterion in specification.Criteria)
		{
			query = query.Where(criterion);
		}

		query = specification.Includes.Aggregate(query,
			(current, include) => current.Include(include));

		query = specification.IncludeStrings.Aggregate(query,
			(current, include) => current.Include(include));

		if (specification.SortExpressions.Count > 0)
		{
			IOrderedQueryable<TEntity>? orderedQuery = null;
			foreach (var (KeySelector, IsDescending) in specification.SortExpressions)
			{
				if (orderedQuery == null)
				{
					orderedQuery = IsDescending
						? query.OrderByDescending(KeySelector)
						: query.OrderBy(KeySelector);
				}
				else
				{
					orderedQuery = IsDescending
						? orderedQuery.ThenByDescending(KeySelector)
						: orderedQuery.ThenBy(KeySelector);
				}
			}
			query = orderedQuery ?? query;
		}
		else if (specification.OrderBy != null)
		{
			query = query.OrderBy(specification.OrderBy);
		}
		else if (specification.OrderByDescending != null)
		{
			query = query.OrderByDescending(specification.OrderByDescending);
		}

		if (specification.GroupBy != null)
		{
			query = query.GroupBy(specification.GroupBy).SelectMany(x => x);
		}

		if (specification.IsNoTracking)
		{
			query = query.AsNoTracking();
		}

		return query;
	}
}
