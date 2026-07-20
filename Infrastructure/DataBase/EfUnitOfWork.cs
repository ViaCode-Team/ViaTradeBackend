using Application.Common.Interfaces;

namespace Infrastructure.DataBase;

public class EfUnitOfWork(AppDbContext context) : IUnitOfWork
{
	public async Task<int> SaveChangesAsync(CancellationToken ct = default)
	{
		return await context.SaveChangesAsync(ct);
	}
}
