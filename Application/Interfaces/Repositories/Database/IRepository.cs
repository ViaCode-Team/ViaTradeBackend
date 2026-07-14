using Domain.Models.Pagination;
using System.Linq.Expressions;

namespace Application.Interfaces.Repositories.Database;

public interface IRepository<TEntity, TDto> where TEntity : class where TDto : class
{
	Task<TEntity?> GetByIdAsync(int id, CancellationToken ct = default);
	Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken ct = default);
	Task<PagedResult<TEntity>> GetPagedAsync(PaginationRequest paginationRequest, CancellationToken ct = default);
	Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default);
	Task<PagedResult<TEntity>> FindPagedAsync(Expression<Func<TEntity, bool>> predicate, PaginationRequest paginationRequest, CancellationToken ct = default);
	Task AddAsync(TEntity entity, CancellationToken ct = default);
	void Update(TEntity entity);
	void Remove(TEntity entity);
	Task<int> SaveChangesAsync(CancellationToken ct = default);

	Task<IEnumerable<TDto>> GetProjectedAsync(Expression<Func<TEntity, TDto>> projection, CancellationToken ct = default);
	Task<int> ExecuteDeleteAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default);
}
