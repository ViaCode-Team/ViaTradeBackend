namespace Domain.Trades.Entities;

public class ScreenerData
{
	public required DateTime Begin { get; set; }

	public required decimal Open { get; set; }

	public required decimal High { get; set; }

	public required decimal Low { get; set; }

	public required decimal Close { get; set; }

	public required long Volume { get; set; }

	public decimal? Ema20 { get; set; }

	public decimal? Ema50 { get; set; }

	public decimal? Ema200 { get; set; }

	public decimal? Rsi14 { get; set; }

	public Dictionary<string, decimal?> AdditionalIndicators { get; set; } = new();
}
