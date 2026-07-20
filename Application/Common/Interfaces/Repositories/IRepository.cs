using System.Linq.Expressions;
using Application.Common.Queries;
using Domain.Entities;

namespace Application.Common.Interfaces.Repositories;

public interface IRepository<TEntity>
	where TEntity : BaseEntity<int>
{
	Task<TEntity?> GetByIdAsync(int id, CancellationToken ct = default);
	Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken ct = default);
	Task<PageResult<TEntity>> GetPagedAsync(PageOptions page, CancellationToken ct = default);
	Task<PageResult<TEntity>> GetPagedAsync(
		IQuerySpecification<TEntity> spec,
		PageOptions page,
		CancellationToken ct = default
	);
	Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default);
	Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default);
	Task<PageResult<TEntity>> FindPagedAsync(
		Expression<Func<TEntity, bool>> predicate,
		PageOptions page,
		CancellationToken ct = default
	);
	Task AddAsync(TEntity entity, CancellationToken ct = default);
	void Update(TEntity entity);
	void Remove(TEntity entity);
	Task<int> ExecuteDeleteAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default);
	Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default);
	Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default);
}
