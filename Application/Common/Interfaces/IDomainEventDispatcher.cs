using Domain.Common;

namespace Application.Common.Interfaces;

public interface IDomainEventDispatcher
{
	Task DispatchAndClearEvents(IEnumerable<IHasDomainEvents> entitiesWithEvents);
}
