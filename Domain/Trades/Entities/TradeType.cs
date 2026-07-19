using Domain.Common;

namespace Domain.Trades.Entities;

public sealed class TradeType : BaseEntity<int>
{
	public string Name { get; set; }

	public ICollection<Trade>? Trades { get; set; }

}
