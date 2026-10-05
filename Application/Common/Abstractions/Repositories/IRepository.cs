using System.Linq.Expressions;
using Ardalis.Specification;
using ViaTrade.Application.Common.Models;
using ViaTrade.Application.Common.Specifications;

namespace ViaTrade.Application.Common.Abstractions.Repositories;

public interface IRepository<T> : IReadRepository<T>
	where T : class
{
	/// <summary>
	/// Adds an entity.
	/// </summary>
	/// <param name="entity">The entity to add.</param>
	void Add(T entity);

	void Update(T entity);

	/// <summary>
	/// Adds the given entities.
	/// </summary>
	/// <param name="entities">The entities to add.</param>
	void AddRange(IEnumerable<T> entities);

	/// <summary>
	/// Adds the given entities.
	/// </summary>
	/// <param name="entities">The entities to add.</param>
	void AddRange(params T[] entities);

	/// <summary>
	/// Removes an entity.
	/// </summary>
	/// <param name="entity">The entity to remove.</param>
	void Remove(T entity);

	/// <summary>
	/// Removes the given entities.
	/// </summary>
	/// <param name="entities">The entities to remove.</param>
	void RemoveRange(IEnumerable<T> entities);

	/// <summary>
	/// Removes the given entities.
	/// </summary>
	/// <param name="entities">The entities to remove.</param>
	void RemoveRange(params T[] entities);

	/// <summary>
	/// Removes all entities of <typeparamref name="T" /> that match the encapsulated query logic
	/// of the <paramref name="specification"/>.
	/// </summary>
	/// <param name="specification">The encapsulated query logic.</param>
	void RemoveRange(ISpecification<T> specification);

	Task<int> ExecuteDeleteAsync(ISpecification<T> specification, CancellationToken ct);

	Task<int> ExecuteDeleteAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);

	/// <summary>
	/// Persists all pending changes to the database.
	/// </summary>
	/// <param name="cancellationToken">The cancellation token.</param>
	/// <returns>
	/// A task that represents the asynchronous save operation.
	/// The task result contains the number of state entries written to the database.
	/// </returns>
	Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

/// <inheritdoc/>
public interface IReadRepository<T> : IReadRepositoryBase<T>
	where T : class
{
	Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);

	Task<TResult?> FirstOrDefaultAsync<TResult>(
		Expression<Func<T, bool>> predicate,
		Expression<Func<T, TResult>> selector,
		CancellationToken ct = default
	);

	Task<T?> SingleOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);

	Task<TResult?> SingleOrDefaultAsync<TResult>(
		Expression<Func<T, bool>> predicate,
		Expression<Func<T, TResult>> selector,
		CancellationToken ct = default
	);

	Task<List<T>> ListAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);

	Task<List<TResult>> ListAsync<TResult>(
		Expression<Func<T, bool>> predicate,
		Expression<Func<T, TResult>> selector,
		CancellationToken ct = default
	);

	Task<PageResult<T>> GetPageAsync(PageSpecification<T> specification, CancellationToken ct = default);

	Task<PageResult<TResult>> GetPageAsync<TResult>(
		PageSpecification<T> specification,
		Expression<Func<T, TResult>> selector,
		CancellationToken ct = default
	);

	Task<int> CountAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);

	Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);

	IAsyncEnumerable<T> AsAsyncEnumerable(Expression<Func<T, bool>> predicate);
}
