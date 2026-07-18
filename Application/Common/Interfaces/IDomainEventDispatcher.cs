using Domain.Common;

namespace Application.Common.Interfaces;

public interface IDomainEventDispatcher
{
    Task DispatchAndClearEvents(IEnumerable<Entity> entitiesWithEvents);
}
