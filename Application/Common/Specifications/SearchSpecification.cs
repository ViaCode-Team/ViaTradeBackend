using ViaTrade.Application.Common.Interfaces;

namespace ViaTrade.Application.Common.Specifications;

public abstract class SearchSpecification<TEntity, TFilter> : ISearchSpecification<TEntity>
	where TEntity : class
	where TFilter : class
{
	protected TFilter Filter { get; }

	protected SearchSpecification(TFilter filter) => Filter = filter;

	public abstract IQueryable<TEntity> Apply(IQueryable<TEntity> query);
}
