namespace Domain.Trades.Entities;

public class TradingInstrumentData
{
	public required string InstrumentCode { get; set; }

	public required TradeDataType DataType { get; set; }

	public List<TradeBar> Bars { get; set; } = new();

	public List<StrategyResult>? StrategyResults { get; set; }

	public List<ScreenerData>? ScreenerData { get; set; }
}
