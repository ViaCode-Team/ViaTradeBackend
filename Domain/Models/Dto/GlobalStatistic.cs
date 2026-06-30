namespace Domain.Models.Dto
{
    public class GlobalStatistic
    {
        public required TradeStatistic TradeStatistic { get; set; }

        public required IncomeTradeStatistic IncomeStatistic { get; set; }

        public required WinrateTradeStatistic WinrateStatistic { get; set; }
    }
}
