using Application.Common.Models.Pagination;
using Domain.Common;
using System.Linq.Expressions;

namespace Application.Common.Interfaces.Repositories;

public interface IRepository<TEntity> where TEntity : BaseEntity<int>
{
	Task<TEntity?> GetByIdAsync(int id, CancellationToken ct = default);
	Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken ct = default);
	Task<PagedResult<TEntity>> GetPagedAsync(PaginationRequest? paginationRequest, CancellationToken ct = default);
	Task<PagedResult<TEntity>> GetPagedAsync(IQuerySpecification<TEntity> spec, PaginationRequest? paginationRequest, CancellationToken ct = default);
	Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default);
	Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default);
	Task<PagedResult<TEntity>> FindPagedAsync(Expression<Func<TEntity, bool>> predicate, PaginationRequest? paginationRequest, CancellationToken ct = default);
	Task AddAsync(TEntity entity, CancellationToken ct = default);
	void Update(TEntity entity);
	void Remove(TEntity entity);
	Task<int> ExecuteDeleteAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default);
	Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default);
}
