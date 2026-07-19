using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Application.Common.Models.Pagination;
using Domain.Common;
using Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace Infrastructure.Repositories.DataBase;

public class GenericEfRepository<TEntity> : IRepository<TEntity>
	where TEntity : BaseEntity<int>
{
	protected readonly AppDbContext _context;
	protected readonly DbSet<TEntity> _dbSet;

	public GenericEfRepository(AppDbContext context)
	{
		_context = context;
		_dbSet = _context.Set<TEntity>();
	}

	public async Task<TEntity?> GetByIdAsync(int id, CancellationToken ct = default)
	{
		return await _dbSet.FindAsync([id], ct);
	}

	public async Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken ct = default)
	{
		return await _dbSet.ToListAsync(ct);
	}

	public async Task<PagedResult<TEntity>> GetPagedAsync(PaginationRequest? paginationRequest, CancellationToken ct = default)
	{
		return await _dbSet.OrderBy(e => e.Id).ToPagedAsync(paginationRequest, ct);
	}

	public async Task<PagedResult<TEntity>> GetPagedAsync(IQuerySpecification<TEntity> spec, PaginationRequest? paginationRequest, CancellationToken ct = default)
	{
		var query = SpecificationEvaluator.GetQuery(_dbSet.AsQueryable(), spec);
		if (spec.SortExpressions.Count == 0)
			query = query.OrderBy(e => e.Id);

		return await query.ToPagedAsync(paginationRequest, ct);
	}

	public async Task<IEnumerable<TEntity>> FindAsync(
		Expression<Func<TEntity, bool>> predicate,
		CancellationToken ct = default)
	{
		return await _dbSet.Where(predicate).ToListAsync(ct);
	}

	public async Task<TEntity?> FirstOrDefaultAsync(
		Expression<Func<TEntity, bool>> predicate,
		CancellationToken ct = default)
	{
		return await _dbSet.FirstOrDefaultAsync(predicate, ct);
	}

	public async Task<PagedResult<TEntity>> FindPagedAsync(
		Expression<Func<TEntity, bool>> predicate,
		PaginationRequest? paginationRequest,
		CancellationToken ct = default)
	{
		return await _dbSet.Where(predicate).OrderBy(e => e.Id).ToPagedAsync(paginationRequest, ct);
	}

	public async Task<bool> ExistsAsync(
		Expression<Func<TEntity, bool>> predicate,
		CancellationToken ct = default)
	{
		return await _dbSet.AnyAsync(predicate, ct);
	}

	public async Task AddAsync(TEntity entity, CancellationToken ct = default)
	{
		await _dbSet.AddAsync(entity, ct);
	}

	public void Update(TEntity entity) => _dbSet.Update(entity);

	public void Remove(TEntity entity) => _dbSet.Remove(entity);

	public async Task<int> ExecuteDeleteAsync(
		Expression<Func<TEntity, bool>> predicate,
		CancellationToken ct = default)
	{
		return await _dbSet.Where(predicate).ExecuteDeleteAsync(ct);
	}

	public async Task<int> ExecuteUpdateAsync(
		Expression<Func<TEntity, bool>> predicate,
		Expression<Func<SetPropertyCalls<TEntity>, SetPropertyCalls<TEntity>>> updateExpression,
		CancellationToken ct = default)
	{
		return await _dbSet.Where(predicate).ExecuteUpdateAsync(updateExpression, ct);
	}
}
