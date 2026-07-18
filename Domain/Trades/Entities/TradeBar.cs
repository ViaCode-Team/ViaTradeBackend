namespace Domain.Trades.Entities;

public class TradeBar
{
	public DateTime Begin { get; set; }

	public decimal Open { get; set; }

	public decimal High { get; set; }

	public decimal Low { get; set; }

	public decimal Close { get; set; }

	public long Volume { get; set; }
}
