using ViaTrade.Application.Common.Interfaces;

namespace ViaTrade.Infrastructure.DataBase;

public class EfUnitOfWork(AppDbContext context) : IUnitOfWork
{
	public Task<int> SaveChangesAsync(CancellationToken ct)
	{
		return EfDatabaseOperation.ExecuteAsync(() => context.SaveChangesAsync(ct));
	}
}
