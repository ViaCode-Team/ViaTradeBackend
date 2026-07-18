using Application.Common.Interfaces;
using Domain.Common;

namespace Infrastructure.Repositories.DataBase;

public class EfUnitOfWork(AppDbContext context, IDomainEventDispatcher dispatcher) : IUnitOfWork
{
	public async Task<int> SaveChangesAsync(CancellationToken ct = default)
	{
		var entitiesWithEvents = context.ChangeTracker
			.Entries<IHasDomainEvents>()
			.Select(e => e.Entity)
			.Where(e => e.DomainEvents.Count > 0)
			.ToList();

		await dispatcher.DispatchAndClearEvents(entitiesWithEvents);

		return await context.SaveChangesAsync(ct);
	}
}
