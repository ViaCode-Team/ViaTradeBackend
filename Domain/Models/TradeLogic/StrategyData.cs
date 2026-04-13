namespace Domain.Models.TradeLogic
{
    public class StrategyData
    {
        public required string Name { get; set; }
        public List<TickerResults> Tickers { get; set; } = new();
    }
}
