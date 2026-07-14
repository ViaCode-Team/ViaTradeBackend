namespace Domain.Models.Dto.Strategy
{
    public class UserStrategyTradeCodeDto
    {
        public required int UserId { get; set; }
        public required int TradeCodeId { get; set; }
        public required int StrategyId { get; set; }
    }
}