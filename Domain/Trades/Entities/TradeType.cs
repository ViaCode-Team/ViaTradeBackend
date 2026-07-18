using Domain.Common;

namespace Domain.Trades.Entities;

public sealed class TradeType : AggregateRoot<int>
{
	public string Name { get; private set; }

	public ICollection<Trade>? Trades { get; private set; }

	private TradeType() { }

	public TradeType(string name)
	{
		Name = name;
	}
}
