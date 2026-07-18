using MediatR;

namespace Domain.Common;

public abstract record DomainEvent : INotification
{
	public DateTime OccurredOn { get; protected set; } = DateTime.UtcNow;
}
