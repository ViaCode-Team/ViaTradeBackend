namespace Application.Common.Interfaces.Repositories;

/// <summary>
/// Executes queries using repositories resolved from separate DI scopes.
/// </summary>
public interface ISeparateContextQueryExecutor
{
	/// <summary>
	/// Executes an independent read-only query using a separate DbContext.
	/// </summary>
	/// <remarks>
	/// Use this method when:
	/// <list type="bullet">
	/// <item>
	/// <description>
	/// Several large queries need to run in parallel, since a single DbContext
	/// does not support concurrent operations.
	/// </description>
	/// </item>
	/// <item>
	/// <description>
	/// A long-running read query must be isolated from the current DbContext.
	/// </description>
	/// </item>
	/// </list>
	///
	/// Do not use this method when:
	/// <list type="bullet">
	/// <item>
	/// <description>
	/// Queries are small and can be combined into a single query using joins or subqueries.
	/// </description>
	/// </item>
	/// <item>
	/// <description>
	/// Queries depend on each other's results and therefore cannot run in parallel.
	/// </description>
	/// </item>
	/// <item>
	/// <description>
	/// The operation modifies data, as concurrent writes may produce inconsistent
	/// or unpredictable results.
	/// </description>
	/// </item>
	/// </list>
	/// </remarks>
	Task<TResult> ExecuteAsync<TRepository, TResult>(
		Func<TRepository, CancellationToken, Task<TResult>> query,
		CancellationToken ct = default
	)
		where TRepository : class;
}
