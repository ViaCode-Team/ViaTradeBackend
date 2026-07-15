using System.Linq.Expressions;

namespace Domain.Interfaces;

public interface ISpecification<T>
{
	List<Expression<Func<T, bool>>> Criteria { get; }
	List<Expression<Func<T, object>>> Includes { get; }
	List<string> IncludeStrings { get; }
	Expression<Func<T, object>>? OrderBy { get; }
	Expression<Func<T, object>>? OrderByDescending { get; }
	Expression<Func<T, object>>? GroupBy { get; }
	bool IsNoTracking { get; }
}
