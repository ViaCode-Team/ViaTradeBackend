namespace ViaTrade.Domain.Interfaces;

public interface IEntity<TId>
{
	TId Id { get; }

	DateTime CreatedAt { get; }
}
