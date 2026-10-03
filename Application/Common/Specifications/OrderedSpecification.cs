using System.Linq.Expressions;
using Ardalis.Specification;

namespace ViaTrade.Application.Common.Specifications;

public abstract class OrderedSpecification<T> : Specification<T>
{
	private IOrderedSpecificationBuilder<T>? _orderedQuery;

	protected void AddOrderByAscending(Expression<Func<T, object?>> keySelector)
	{
		if (_orderedQuery == null)
			_orderedQuery = Query.OrderBy(keySelector);
		else
			_orderedQuery = _orderedQuery.ThenBy(keySelector);
	}

	protected void AddOrderByDescending(Expression<Func<T, object?>> keySelector)
	{
		if (_orderedQuery == null)
			_orderedQuery = Query.OrderByDescending(keySelector);
		else
			_orderedQuery = _orderedQuery.ThenByDescending(keySelector);
	}
}
