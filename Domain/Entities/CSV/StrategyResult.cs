namespace Domain.Entities.CSV
{
    public class StrategyResult
    {
        public DateTime Begin { get; set; }
        public decimal Close { get; set; }
        public required string Signal { get; set; } // HOLD/BUY/SELL
    }
}
