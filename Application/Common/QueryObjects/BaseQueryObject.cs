using System.Linq.Expressions;
using ViaTrade.Application.Common.Interfaces;

namespace ViaTrade.Application.Common.QueryObjects;

public abstract class BaseQueryObject<T> : IQueryObject<T>
{
	public List<Expression<Func<T, bool>>> Criteria { get; } = [];
	public List<Expression<Func<T, object>>> Includes { get; } = [];
	public List<(Expression<Func<T, object>> KeySelector, bool IsDescending)> SortExpressions { get; } = [];
	public bool IsSplitQuery { get; private set; }

	protected BaseQueryObject(Expression<Func<T, bool>> criteria)
	{
		Criteria.Add(criteria);
	}

	protected BaseQueryObject() { }

	protected void AddCriteria(Expression<Func<T, bool>> criteria)
	{
		Criteria.Add(criteria);
	}

	protected void AddInclude(Expression<Func<T, object>> includeExpression)
	{
		Includes.Add(includeExpression);
	}

	protected void AddOrderByAscending(Expression<Func<T, object>> keySelector)
	{
		SortExpressions.Add((keySelector, false));
	}

	protected void AddOrderByDescending(Expression<Func<T, object>> keySelector)
	{
		SortExpressions.Add((keySelector, true));
	}

	protected void ApplySplitQuery()
	{
		IsSplitQuery = true;
	}
}
