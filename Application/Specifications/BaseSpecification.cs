using Domain.Interfaces;
using System.Linq.Expressions;

namespace Application.Specifications;

public abstract class BaseSpecification<T> : ISpecification<T>
{
	public List<Expression<Func<T, bool>>> Criteria { get; } = [];
	public List<Expression<Func<T, object>>> Includes { get; } = [];
	public List<string> IncludeStrings { get; } = [];
	public Expression<Func<T, object>>? OrderBy { get; private set; }
	public Expression<Func<T, object>>? OrderByDescending { get; private set; }
	public List<(Expression<Func<T, object>> KeySelector, bool IsDescending)> SortExpressions { get; } = [];
	public Expression<Func<T, object>>? GroupBy { get; private set; }
	public bool IsNoTracking { get; private set; }

	protected BaseSpecification(Expression<Func<T, bool>> criteria)
	{
		Criteria.Add(criteria);
	}

	protected BaseSpecification()
	{
	}

	protected void AddCriteria(Expression<Func<T, bool>> criteria)
	{
		Criteria.Add(criteria);
	}

	protected void AddInclude(Expression<Func<T, object>> includeExpression)
	{
		Includes.Add(includeExpression);
	}

	protected void AddInclude(string includeString)
	{
		IncludeStrings.Add(includeString);
	}

	protected void ApplyOrderBy(Expression<Func<T, object>> orderByExpression)
	{
		OrderBy = orderByExpression;
	}

	protected void ApplyOrderByDescending(Expression<Func<T, object>> orderByDescExpression)
	{
		OrderByDescending = orderByDescExpression;
	}

	protected void AddOrderBy(Expression<Func<T, object>> keySelector, bool descending = false)
	{
		SortExpressions.Add((keySelector, descending));
	}

	protected void ApplyGroupBy(Expression<Func<T, object>> groupByExpression)
	{
		GroupBy = groupByExpression;
	}

	protected void ApplyNoTracking()
	{
		IsNoTracking = true;
	}
}
