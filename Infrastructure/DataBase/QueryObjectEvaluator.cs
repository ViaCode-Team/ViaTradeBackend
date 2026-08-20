using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using ViaTrade.Application.Common.Interfaces;
using ViaTrade.Domain.Entities;

namespace ViaTrade.Infrastructure.DataBase;

public static class QueryObjectEvaluator
{
	public static IQueryable<TEntity> GetQuery<TEntity>(IQueryable<TEntity> query, IQueryObject<TEntity> queryObject)
		where TEntity : BaseEntity<int>
	{
		if (queryObject.IsSplitQuery)
			query = query.AsSplitQuery();

		query = ApplyCriteria(query, queryObject);
		query = ApplyIncludes(query, queryObject);

		if (queryObject.SortExpressions.Count > 0)
			query = ApplySorting(query, queryObject);

		return query;
	}

	public static IOrderedQueryable<TEntity> GetQueryForPagination<TEntity>(
		IQueryable<TEntity> query,
		IQueryObject<TEntity> queryObject,
		IEntityType entityType
	)
		where TEntity : BaseEntity<int>
	{
		if (queryObject.IsSplitQuery)
			query = query.AsSplitQuery();

		query = ApplyCriteria(query, queryObject);
		query = ApplyIncludes(query, queryObject);

		return ApplyPaginationSorting(query, queryObject, entityType);
	}

	private static IQueryable<TEntity> ApplyCriteria<TEntity>(
		IQueryable<TEntity> query,
		IQueryObject<TEntity> queryObject
	)
		where TEntity : BaseEntity<int>
	{
		foreach (var criterion in queryObject.Criteria)
			query = query.Where(criterion);

		return query;
	}

	private static IQueryable<TEntity> ApplyIncludes<TEntity>(
		IQueryable<TEntity> query,
		IQueryObject<TEntity> queryObject
	)
		where TEntity : BaseEntity<int>
	{
		foreach (var include in queryObject.Includes)
			query = query.Include(include);

		return query;
	}

	private static IOrderedQueryable<TEntity> ApplySorting<TEntity>(
		IQueryable<TEntity> query,
		IQueryObject<TEntity> queryObject
	)
		where TEntity : BaseEntity<int>
	{
		IOrderedQueryable<TEntity> orderedQuery;

		var (keySelector, isDescending) = queryObject.SortExpressions[0];
		if (isDescending)
			orderedQuery = query.OrderByDescending(keySelector);
		else
			orderedQuery = query.OrderBy(keySelector);

		for (var i = 1; i < queryObject.SortExpressions.Count; i++)
		{
			(keySelector, isDescending) = queryObject.SortExpressions[i];
			if (isDescending)
				orderedQuery = orderedQuery.ThenByDescending(keySelector);
			else
				orderedQuery = orderedQuery.ThenBy(keySelector);
		}

		return orderedQuery;
	}

	private static IOrderedQueryable<TEntity> ApplyPaginationSorting<TEntity>(
		IQueryable<TEntity> query,
		IQueryObject<TEntity> queryObject,
		IEntityType entityType
	)
		where TEntity : BaseEntity<int>
	{
		if (queryObject.SortExpressions.Count == 0)
			return query.OrderBy(entity => entity.Id);

		var orderedQuery = ApplySorting(query, queryObject);
		if (!HasUniqueKeySorting(queryObject, entityType))
			orderedQuery = orderedQuery.ThenBy(entity => entity.Id);

		return orderedQuery;
	}

	private static bool HasUniqueKeySorting<TEntity>(
		IQueryObject<TEntity> queryObject,
		IEntityType entityType
	)
		where TEntity : BaseEntity<int>
	{
		var sortedMembers = GetSortedMemberNames(queryObject);
		if (sortedMembers.Count == 0)
			return false;

		foreach (var key in entityType.GetKeys())
		{
			if (IsSubsetOf(key.Properties, sortedMembers))
				return true;
		}

		foreach (var index in entityType.GetIndexes())
		{
			if (index.IsUnique && IsSubsetOf(index.Properties, sortedMembers))
				return true;
		}

		return false;
	}

	private static HashSet<string> GetSortedMemberNames<TEntity>(IQueryObject<TEntity> queryObject)
	{
		var members = new HashSet<string>();

		foreach (var (keySelector, _) in queryObject.SortExpressions)
		{
			Expression body = keySelector.Body;

			if (body is UnaryExpression { NodeType: ExpressionType.Convert } unaryExpression)
				body = unaryExpression.Operand;

			if (body is MemberExpression { Expression: ParameterExpression } memberExpression)
				members.Add(memberExpression.Member.Name);
		}

		return members;
	}

	private static bool IsSubsetOf(IReadOnlyList<IProperty> properties, HashSet<string> sortedMembers)
	{
		foreach (var property in properties)
		{
			if (!sortedMembers.Contains(property.Name))
				return false;
		}

		return true;
	}
}
