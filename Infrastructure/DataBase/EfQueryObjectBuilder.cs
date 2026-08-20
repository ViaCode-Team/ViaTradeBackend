using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using ViaTrade.Application.Common.Interfaces;
using ViaTrade.Domain.Entities;

namespace ViaTrade.Infrastructure.DataBase;

public class EfQueryObjectBuilder(AppDbContext context)
{
	public IQueryable<TEntity> Build<TEntity>(IQueryable<TEntity> query, IQueryObject<TEntity> queryObject)
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

	public (IQueryable<TEntity> Query, bool IsUniqueKeyFilter) BuildForPagination<TEntity>(
		IQueryable<TEntity> query,
		IQueryObject<TEntity> queryObject
	)
		where TEntity : BaseEntity<int>
	{
		if (queryObject.IsSplitQuery)
			query = query.AsSplitQuery();

		query = ApplyCriteria(query, queryObject);
		query = ApplyIncludes(query, queryObject);

		var isUnique = HasUniqueKeyFilter(queryObject);
		if (isUnique)
			return (query, isUnique);

		return (ApplyPaginationSorting(query, queryObject), isUnique);
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

	private IOrderedQueryable<TEntity> ApplyPaginationSorting<TEntity>(
		IQueryable<TEntity> query,
		IQueryObject<TEntity> queryObject
	)
		where TEntity : BaseEntity<int>
	{
		if (queryObject.SortExpressions.Count == 0)
			return query.OrderBy(entity => entity.Id);

		var orderedQuery = ApplySorting(query, queryObject);
		if (!HasUniqueKeySorting(queryObject))
			orderedQuery = orderedQuery.ThenBy(entity => entity.Id);

		return orderedQuery;
	}

	private bool HasUniqueKeySorting<TEntity>(IQueryObject<TEntity> queryObject)
		where TEntity : BaseEntity<int>
	{
		if (queryObject.SortExpressions.Count == 0)
			return false;

		var sortedMembers = new List<string>(queryObject.SortExpressions.Count);

		foreach (var (keySelector, _) in queryObject.SortExpressions)
		{
			Expression body = keySelector.Body;

			if (body is UnaryExpression { NodeType: ExpressionType.Convert } unaryExpression)
				body = unaryExpression.Operand;

			if (body is MemberExpression { Expression: ParameterExpression } memberExpression)
			{
				var memberName = memberExpression.Member.Name;
				if (!sortedMembers.Contains(memberName))
					sortedMembers.Add(memberName);
			}
		}

		if (sortedMembers.Count == 0)
			return false;

		var uniqueKeys = GetUniqueKeyPropertyNames<TEntity>();

		foreach (var uniqueKey in uniqueKeys)
		{
			if (IsSubsetOf(uniqueKey, sortedMembers))
				return true;
		}

		return false;
	}

	public bool HasUniqueKeyFilter<TEntity>(IQueryObject<TEntity> queryObject)
		where TEntity : BaseEntity<int>
	{
		if (queryObject.Criteria.Count == 0)
			return false;

		var filteredMembers = new List<string>(queryObject.Criteria.Count);

		foreach (var criterion in queryObject.Criteria)
		{
			ExtractEqualityMembers(criterion.Body, filteredMembers);
		}

		if (filteredMembers.Count == 0)
			return false;

		var uniqueKeys = GetUniqueKeyPropertyNames<TEntity>();

		foreach (var uniqueKey in uniqueKeys)
		{
			if (IsSubsetOf(uniqueKey, filteredMembers))
				return true;
		}

		return false;
	}

	private static void ExtractEqualityMembers(Expression expr, List<string> members)
	{
		while (expr.NodeType == ExpressionType.AndAlso)
		{
			var binary = (BinaryExpression)expr;

			ExtractEqualityMembers(binary.Left, members);
			expr = binary.Right;
		}

		if (expr.NodeType == ExpressionType.Equal)
		{
			var binary = (BinaryExpression)expr;
			if (IsNullConstant(binary.Left) || IsNullConstant(binary.Right))
				return;

			var memberName = GetMemberName(binary.Left) ?? GetMemberName(binary.Right);
			if (memberName != null && !members.Contains(memberName))
			{
				members.Add(memberName);
			}
		}
	}

	private static string? GetMemberName(Expression expr)
	{
		if (expr.NodeType == ExpressionType.Convert)
			expr = ((UnaryExpression)expr).Operand;

		if (expr is MemberExpression memberExpr && memberExpr.Expression is ParameterExpression)
		{
			return memberExpr.Member.Name;
		}

		return null;
	}

	private static bool IsNullConstant(Expression expr)
	{
		if (expr.NodeType == ExpressionType.Convert)
			expr = ((UnaryExpression)expr).Operand;

		return expr is ConstantExpression { Value: null } or DefaultExpression;
	}

	private static bool IsSubsetOf(string[] uniqueKeyProperties, List<string> members)
	{
		foreach (var property in uniqueKeyProperties)
		{
			if (!members.Contains(property))
				return false;
		}

		return true;
	}

	private string[][] GetUniqueKeyPropertyNames<TEntity>()
		where TEntity : BaseEntity<int>
	{
		var entityType =
			context.Model.FindEntityType(typeof(TEntity))
			?? throw new InvalidOperationException($"Entity type {typeof(TEntity).Name} not found in model.");

		var uniqueKeys = new List<string[]>();

		foreach (var key in entityType.GetKeys())
		{
			uniqueKeys.Add(key.Properties.Select(p => p.Name).ToArray());
		}

		foreach (var index in entityType.GetIndexes())
		{
			if (index.IsUnique)
			{
				uniqueKeys.Add(index.Properties.Select(p => p.Name).ToArray());
			}
		}

		return uniqueKeys.ToArray();
	}
}
