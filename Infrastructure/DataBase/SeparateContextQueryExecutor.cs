using Application.Common.Interfaces.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.DataBase;

public class SeparateContextQueryExecutor(IServiceScopeFactory scopeFactory) : ISeparateContextQueryExecutor
{
	public async Task<TResult> ExecuteAsync<TRepository, TResult>(
		Func<TRepository, CancellationToken, Task<TResult>> query,
		CancellationToken ct = default
	)
		where TRepository : class
	{
		await using var scope = scopeFactory.CreateAsyncScope();
		var repository = scope.ServiceProvider.GetRequiredService<TRepository>();

		return await query(repository, ct);
	}
}
