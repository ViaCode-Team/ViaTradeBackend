namespace Domain.Entities.DataBase
{
    public class UserTradeStrategy : BaseEntity
    {
        public int UserId { get; set; }
        public int TradeStrategyId { get; set; }

        public required User User { get; set; }
        public required TradeStrategy TradeStrategy { get; set; }
    }
}
