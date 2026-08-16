using Domain.Interfaces;

namespace Domain.Entities;

public abstract class BaseEntity<TId> : IEntity<TId>
{
	public TId Id { get; protected set; } = default!;

	public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;
}
