namespace Domain.Entities.CSV;

public class ScreenerData
{
	public DateTime Begin { get; set; }
	public decimal Open { get; set; }
	public decimal High { get; set; }
	public decimal Low { get; set; }
	public decimal Close { get; set; }
	public long Volume { get; set; }
	public decimal? Ema20 { get; set; }
	public decimal? Ema50 { get; set; }
	public decimal? Ema200 { get; set; }
	public decimal? Rsi14 { get; set; }
	public Dictionary<string, decimal?> AdditionalIndicators { get; set; } = new();
}
