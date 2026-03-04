namespace Domain.Entities.CSV
{
    public class TradingInstrumentData
    {
        public string InstrumentCode { get; set; } = string.Empty;
        public DataType DataType { get; set; }
        public List<TradeBar> Bars { get; set; } = new();
        public List<StrategyResult>? StrategyResults { get; set; }
        public List<ScreenerData>? ScreenerData { get; set; }
    }
}
}
