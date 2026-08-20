using System.Linq.Expressions;

namespace ViaTrade.Application.Common.Interfaces;

public interface IQueryObject<T>
{
	List<Expression<Func<T, bool>>> Criteria { get; }
	List<Expression<Func<T, object>>> Includes { get; }
	List<(Expression<Func<T, object>> KeySelector, bool IsDescending)> SortExpressions { get; }
	bool IsSplitQuery { get; }
}
