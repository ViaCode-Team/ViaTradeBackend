using System.Linq.Expressions;

namespace Domain.Interfaces;

public interface IQuerySpecification<T>
{
	List<Expression<Func<T, bool>>> Criteria { get; }
	List<Expression<Func<T, object>>> Includes { get; }
	List<(Expression<Func<T, object>> KeySelector, bool IsDescending)> SortExpressions { get; }
	bool IsSplitQuery { get; }
}
