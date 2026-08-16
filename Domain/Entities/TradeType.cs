namespace ViaTrade.Domain.Entities;

public sealed class TradeType : BaseEntity<int>
{
	public required string Name { get; set; }

	public ICollection<Trade> Trades { get; set; } = [];
}
