using System.Linq.Expressions;
using Ardalis.Specification;
using Ardalis.Specification.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ViaTrade.Application.Common.Interfaces.Repositories;

namespace ViaTrade.Infrastructure.DataBase.Repositories.Generic;

public class ReadEfRepository<T>(AppDbContext context, ISpecificationEvaluator specificationEvaluator)
	: IReadRepository<T>
	where T : class
{
	protected readonly DbContext _context = context;
	protected readonly DbSet<T> _dbSet = context.Set<T>();
	protected readonly ISpecificationEvaluator _specificationEvaluator = specificationEvaluator;

	protected virtual IQueryable<T> Query => _dbSet;

	public ReadEfRepository(AppDbContext dbContext)
		: this(dbContext, SpecificationEvaluator.Default) { }

	public async Task<T?> GetByIdAsync<TId>(TId id, CancellationToken ct)
		where TId : notnull
	{
		var key = _context.Model.FindEntityType(typeof(T))!.FindPrimaryKey()!.Properties.Single();

		return await Query.FirstOrDefaultAsync(entity => EF.Property<TId>(entity, key.Name)!.Equals(id), ct);
	}

	public async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken ct)
	{
		return await Query.FirstOrDefaultAsync(predicate, ct);
	}

	public Task<TResult?> FirstOrDefaultAsync<TResult>(
		Expression<Func<T, bool>> predicate,
		Expression<Func<T, TResult>> selector,
		CancellationToken ct
	)
	{
		return Query.Where(predicate).Select(selector).FirstOrDefaultAsync(ct);
	}

	public async Task<T?> FirstOrDefaultAsync(ISpecification<T> specification, CancellationToken ct)
	{
		return await ApplySpecification(specification).FirstOrDefaultAsync(ct);
	}

	public async Task<TResult?> FirstOrDefaultAsync<TResult>(
		ISpecification<T, TResult> specification,
		CancellationToken ct
	)
	{
		return await ApplySpecification(specification).FirstOrDefaultAsync(ct);
	}

	public async Task<T?> SingleOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken ct)
	{
		return await Query.SingleOrDefaultAsync(predicate, ct);
	}

	public Task<TResult?> SingleOrDefaultAsync<TResult>(
		Expression<Func<T, bool>> predicate,
		Expression<Func<T, TResult>> selector,
		CancellationToken ct
	)
	{
		return Query.Where(predicate).Select(selector).SingleOrDefaultAsync(ct);
	}

	public async Task<T?> SingleOrDefaultAsync(ISingleResultSpecification<T> specification, CancellationToken ct)
	{
		return await ApplySpecification(specification).SingleOrDefaultAsync(ct);
	}

	public async Task<TResult?> SingleOrDefaultAsync<TResult>(
		ISingleResultSpecification<T, TResult> specification,
		CancellationToken ct
	)
	{
		return await ApplySpecification(specification).SingleOrDefaultAsync(ct);
	}

	public async Task<List<T>> ListAsync(CancellationToken ct)
	{
		return await Query.ToListAsync(ct);
	}

	public async Task<List<T>> ListAsync(Expression<Func<T, bool>> predicate, CancellationToken ct)
	{
		return await Query.Where(predicate).ToListAsync(ct);
	}

	public Task<List<TResult>> ListAsync<TResult>(
		Expression<Func<T, bool>> predicate,
		Expression<Func<T, TResult>> selector,
		CancellationToken ct
	)
	{
		return Query.Where(predicate).Select(selector).ToListAsync(ct);
	}

	public async Task<List<T>> ListAsync(ISpecification<T> specification, CancellationToken ct)
	{
		var queryResult = await ApplySpecification(specification).ToListAsync(ct);

		if (specification.PostProcessingAction != null)
			return specification.PostProcessingAction(queryResult).ToList();

		return queryResult;
	}

	public async Task<List<TResult>> ListAsync<TResult>(ISpecification<T, TResult> specification, CancellationToken ct)
	{
		var queryResult = await ApplySpecification(specification).ToListAsync(ct);

		if (specification.PostProcessingAction != null)
			return specification.PostProcessingAction(queryResult).ToList();

		return queryResult;
	}

	public async Task<int> CountAsync(ISpecification<T> specification, CancellationToken ct)
	{
		return await ApplySpecification(specification, true).CountAsync(ct);
	}

	public async Task<int> CountAsync(CancellationToken ct)
	{
		return await Query.CountAsync(ct);
	}

	public async Task<int> CountAsync(Expression<Func<T, bool>> predicate, CancellationToken ct)
	{
		return await Query.CountAsync(predicate, ct);
	}

	public async Task<bool> AnyAsync(ISpecification<T> specification, CancellationToken ct)
	{
		return await ApplySpecification(specification, true).AnyAsync(ct);
	}

	public async Task<bool> AnyAsync(CancellationToken ct)
	{
		return await Query.AnyAsync(ct);
	}

	public async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken ct)
	{
		return await Query.AnyAsync(predicate, ct);
	}

	public IAsyncEnumerable<T> AsAsyncEnumerable(ISpecification<T> specification)
	{
		return ApplySpecification(specification).AsAsyncEnumerable();
	}

	public IAsyncEnumerable<T> AsAsyncEnumerable(Expression<Func<T, bool>> predicate)
	{
		return Query.Where(predicate).AsAsyncEnumerable();
	}

	/// <summary>
	/// Filters the entities  of <typeparamref name="T"/>, to those that match the encapsulated query logic of the
	/// <paramref name="specification"/>.
	/// </summary>
	/// <param name="specification">The encapsulated query logic.</param>
	/// <param name="evaluateCriteriaOnly">It ignores pagination and evaluators that don't affect Count.</param>
	/// <returns>The filtered entities as an <see cref="IQueryable{T}"/>.</returns>
	protected virtual IQueryable<T> ApplySpecification(
		ISpecification<T> specification,
		bool evaluateCriteriaOnly = false
	)
	{
		return _specificationEvaluator.GetQuery(Query, specification, evaluateCriteriaOnly);
	}

	/// <summary>
	/// Filters all entities of <typeparamref name="T" />, that matches the encapsulated query logic of the
	/// <paramref name="specification"/>, from the database.
	/// <para>
	/// Projects each entity into a new form, being <typeparamref name="TResult" />.
	/// </para>
	/// </summary>
	/// <typeparam name="TResult">The type of the value returned by the projection.</typeparam>
	/// <param name="specification">The encapsulated query logic.</param>
	/// <returns>The filtered projected entities as an <see cref="IQueryable{TResult}"/>.</returns>
	protected virtual IQueryable<TResult> ApplySpecification<TResult>(ISpecification<T, TResult> specification)
	{
		return _specificationEvaluator.GetQuery(Query, specification);
	}
}
