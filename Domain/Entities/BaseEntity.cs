using ViaTrade.Domain.Interfaces;

namespace ViaTrade.Domain.Entities;

public abstract class BaseEntity<TId> : IEntity<TId>
{
	public TId Id { get; protected set; } = default!;

	public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;
}
