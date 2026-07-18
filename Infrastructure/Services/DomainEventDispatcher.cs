using Application.Common.Interfaces;
using Domain.Common;
using MediatR;

namespace Infrastructure.Services;

public class DomainEventDispatcher(IPublisher publisher) : IDomainEventDispatcher
{
    public async Task DispatchAndClearEvents(IEnumerable<Entity> entitiesWithEvents)
    {
        foreach (var entity in entitiesWithEvents)
        {
            var events = entity.DomainEvents.ToArray();
            entity.ClearDomainEvents();
            
            foreach (var domainEvent in events)
            {
                await publisher.Publish(domainEvent).ConfigureAwait(false);
            }
        }
    }
}
