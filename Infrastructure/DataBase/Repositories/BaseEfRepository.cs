using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using ViaTrade.Application.Common.Interfaces;
using ViaTrade.Application.Common.Interfaces.Repositories;
using ViaTrade.Application.Common.Models;
using ViaTrade.Domain.Entities;
using ViaTrade.Infrastructure.DataBase.Extensions;

namespace ViaTrade.Infrastructure.DataBase.Repositories;

public class BaseEfRepository<TEntity> : IRepository<TEntity>
	where TEntity : BaseEntity<int>
{
	protected readonly AppDbContext _context;
	protected readonly DbSet<TEntity> _dbSet;

	public BaseEfRepository(AppDbContext context)
	{
		_context = context;
		_dbSet = _context.Set<TEntity>();
	}

	public async Task<TEntity?> FindByIdAsync(int id, CancellationToken ct)
	{
		return await _dbSet.FindAsync([id], ct);
	}

	public async Task<IReadOnlyList<TEntity>> ListAsync(CancellationToken ct)
	{
		return await _dbSet.ToListAsync(ct);
	}

	public async Task<PageResult<TEntity>> GetPageAsync(PageOptions pageOptions, CancellationToken ct)
	{
		return await _dbSet.OrderBy(e => e.Id).ToPagedAsync(pageOptions, ct);
	}

	public async Task<PageResult<TEntity>> GetPageAsync(
		IQueryObject<TEntity> queryObject,
		PageOptions pageOptions,
		CancellationToken ct
	)
	{
		var query = QueryObjectEvaluator.GetQueryForPagination(_dbSet.AsQueryable(), queryObject);

		return await query.ToPagedAsync(pageOptions, ct);
	}

	public async Task<IReadOnlyList<TEntity>> ListByAsync(
		Expression<Func<TEntity, bool>> predicate,
		CancellationToken ct = default
	)
	{
		return await _dbSet.Where(predicate).ToListAsync(ct);
	}

	public async Task<TEntity?> FindOneAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct)
	{
		return await _dbSet.FirstOrDefaultAsync(predicate, ct);
	}

	public async Task<TEntity?> FindOneAsync(CancellationToken ct)
	{
		return await _dbSet.FirstOrDefaultAsync(ct);
	}

	public async Task<PageResult<TEntity>> GetPageByAsync(
		Expression<Func<TEntity, bool>> predicate,
		PageOptions pageOptions,
		CancellationToken ct
	)
	{
		return await _dbSet.Where(predicate).OrderBy(e => e.Id).ToPagedAsync(pageOptions, ct);
	}

	public async Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct)
	{
		return await _dbSet.AnyAsync(predicate, ct);
	}

	public async Task<bool> ExistsAsync(CancellationToken ct)
	{
		return await _dbSet.AnyAsync(ct);
	}

	public async Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct)
	{
		return await _dbSet.CountAsync(predicate, ct);
	}

	public async Task<int> CountAsync(CancellationToken ct)
	{
		return await _dbSet.CountAsync(ct);
	}

	public async Task AddAsync(TEntity entity, CancellationToken ct)
	{
		await _dbSet.AddAsync(entity, ct);
	}

	public void Update(TEntity entity) => _dbSet.Update(entity);

	public void Remove(TEntity entity) => _dbSet.Remove(entity);

	public Task<int> ExecuteDeleteAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct) =>
		EfDatabaseOperation.ExecuteAsync(() => _dbSet.Where(predicate).ExecuteDeleteAsync(ct));
}
