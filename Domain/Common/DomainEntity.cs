namespace Domain.Common;

public abstract class DomainEntity<TId> : IHasDomainEvents
{
	public TId Id { get; protected set; } = default!;

	public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;

	private readonly List<DomainEvent> _domainEvents = [];

	public IReadOnlyCollection<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();

	protected void AddDomainEvent(DomainEvent domainEvent)
	{
		_domainEvents.Add(domainEvent);
	}

	protected void RemoveDomainEvent(DomainEvent domainEvent)
	{
		_domainEvents.Remove(domainEvent);
	}

	public void ClearDomainEvents()
	{
		_domainEvents.Clear();
	}
}
