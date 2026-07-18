namespace Domain.Trades.Entities;

public class TradingInstrumentData
{
	public string InstrumentCode { get; set; } = string.Empty;

	public TradeDataType DataType { get; set; }

	public List<TradeBar> Bars { get; set; } = new();

	public List<StrategyResult>? StrategyResults { get; set; }

	public List<ScreenerData>? ScreenerData { get; set; }
}
