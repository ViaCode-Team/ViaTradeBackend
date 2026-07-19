namespace Domain.Common;

public abstract class BaseEntity<TId>
{
	public TId Id { get; protected set; } = default!;

	public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;
}
