using System.Linq.Expressions;
using Application.Common.Interfaces;

namespace Application.Common.Specifications;

public abstract class BaseQuerySpecification<T> : IQuerySpecification<T>
{
	public List<Expression<Func<T, bool>>> Criteria { get; } = [];
	public List<Expression<Func<T, object>>> Includes { get; } = [];
	public List<(Expression<Func<T, object>> KeySelector, bool IsDescending)> SortExpressions { get; } = [];
	public bool IsSplitQuery { get; private set; }

	protected BaseQuerySpecification(Expression<Func<T, bool>> criteria)
	{
		Criteria.Add(criteria);
	}

	protected BaseQuerySpecification() { }

	protected void AddCriteria(Expression<Func<T, bool>> criteria)
	{
		Criteria.Add(criteria);
	}

	protected void AddInclude(Expression<Func<T, object>> includeExpression)
	{
		Includes.Add(includeExpression);
	}

	protected void AddOrderBy(Expression<Func<T, object>> keySelector, bool descending = false)
	{
		SortExpressions.Add((keySelector, descending));
	}

	protected void ApplySplitQuery()
	{
		IsSplitQuery = true;
	}
}
