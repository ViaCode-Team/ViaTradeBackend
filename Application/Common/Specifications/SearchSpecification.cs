using ViaTrade.Application.Common.Interfaces;

namespace ViaTrade.Application.Common.Specifications;

public abstract class SearchSpecification<TEntity, TFilter>(TFilter filter) : ISearchSpecification<TEntity>
	where TEntity : class
	where TFilter : class
{
	protected TFilter Filter { get; } = filter;

	public abstract IQueryable<TEntity> Apply(IQueryable<TEntity> query);
}
