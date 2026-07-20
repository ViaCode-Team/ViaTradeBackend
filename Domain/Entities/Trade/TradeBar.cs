namespace Domain.Trades.Entities;

public class TradeBar
{
	public required DateTime Begin { get; set; }

	public required decimal Open { get; set; }

	public required decimal High { get; set; }

	public required decimal Low { get; set; }

	public required decimal Close { get; set; }

	public required long Volume { get; set; }
}
