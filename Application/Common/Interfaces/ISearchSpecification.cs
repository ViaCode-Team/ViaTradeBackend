namespace ViaTrade.Application.Common.Interfaces;

public interface ISearchSpecification<TEntity>
	where TEntity : class
{
	IQueryable<TEntity> Apply(IQueryable<TEntity> query);
}
