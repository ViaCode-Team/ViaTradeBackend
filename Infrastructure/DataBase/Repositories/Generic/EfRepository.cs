using System.Linq.Expressions;
using Ardalis.Specification;
using Ardalis.Specification.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ViaTrade.Application.Common.Abstractions.Repositories;

namespace ViaTrade.Infrastructure.DataBase.Repositories.Generic;

public class EfRepository<TEntity>(AppDbContext context, ISpecificationEvaluator specificationEvaluator)
	: ReadEfRepository<TEntity>(context, specificationEvaluator),
		IRepository<TEntity>
	where TEntity : class
{
	public EfRepository(AppDbContext dbContext)
		: this(dbContext, SpecificationEvaluator.Default) { }

	public void Add(TEntity entity)
	{
		_dbSet.Add(entity);
	}

	public void Update(TEntity entity)
	{
		_dbSet.Update(entity);
	}

	public void AddRange(IEnumerable<TEntity> entities)
	{
		_dbSet.AddRange(entities);
	}

	public void AddRange(params TEntity[] entities)
	{
		_dbSet.AddRange(entities);
	}

	public void Remove(TEntity entity)
	{
		_dbSet.Remove(entity);
	}

	public void RemoveRange(IEnumerable<TEntity> entities)
	{
		_dbSet.RemoveRange(entities);
	}

	public void RemoveRange(params TEntity[] entities)
	{
		_dbSet.RemoveRange(entities);
	}

	public void RemoveRange(ISpecification<TEntity> specification)
	{
		var query = ApplySpecification(specification);
		_dbSet.RemoveRange(query);
	}

	public Task<int> ExecuteDeleteAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct)
	{
		return _dbSet.Where(predicate).ExecuteDeleteAsync(ct);
	}

	public Task<int> ExecuteDeleteAsync(ISpecification<TEntity> specification, CancellationToken ct)
	{
		return ApplySpecification(specification).ExecuteDeleteAsync(ct);
	}

	public async Task<int> SaveChangesAsync(CancellationToken ct)
	{
		return await _context.SaveChangesAsync(ct);
	}
}
