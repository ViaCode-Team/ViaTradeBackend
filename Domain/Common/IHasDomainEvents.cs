namespace Domain.Common;

public interface IHasDomainEvents
{
	IReadOnlyCollection<DomainEvent> DomainEvents { get; }
	void ClearDomainEvents();
}
