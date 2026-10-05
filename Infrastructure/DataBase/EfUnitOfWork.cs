using ViaTrade.Application.Common.Abstractions;

namespace ViaTrade.Infrastructure.DataBase;

public class EfUnitOfWork(AppDbContext context) : IUnitOfWork
{
	public Task<int> SaveChangesAsync(CancellationToken ct)
	{
		return context.SaveChangesAsync(ct);
	}
}
