using System.Linq.Expressions;
using ViaTrade.Application.Common.Models;
using ViaTrade.Domain.Entities;

namespace ViaTrade.Application.Common.Interfaces.Repositories;

public interface IRepository<TEntity>
	where TEntity : BaseEntity<int>
{
	Task<TEntity?> FindByIdAsync(int id, CancellationToken ct = default);
	Task<IReadOnlyList<TEntity>> ListAsync(CancellationToken ct = default);
	Task<PageResult<TEntity>> GetPageAsync(PageOptions pageOptions, CancellationToken ct = default);
	Task<PageResult<TEntity>> GetPageAsync(
		IQuerySpecification<TEntity> spec,
		PageOptions pageOptions,
		CancellationToken ct = default
	);
	Task<IReadOnlyList<TEntity>> ListByAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default);
	Task<TEntity?> FindOneAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default);
	Task<TEntity?> FindOneAsync(CancellationToken ct = default);
	Task<PageResult<TEntity>> GetPageByAsync(
		Expression<Func<TEntity, bool>> predicate,
		PageOptions pageOptions,
		CancellationToken ct = default
	);
	Task AddAsync(TEntity entity, CancellationToken ct = default);
	void Update(TEntity entity);
	void Remove(TEntity entity);
	Task<int> ExecuteDeleteAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default);
	Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default);
	Task<bool> ExistsAsync(CancellationToken ct = default);
	Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default);
	Task<int> CountAsync(CancellationToken ct = default);
}
