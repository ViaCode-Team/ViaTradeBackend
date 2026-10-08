namespace ViaTrade.Domain.Entities.Abstractions;

public interface IEntity<TId>
{
	TId Id { get; }
}
