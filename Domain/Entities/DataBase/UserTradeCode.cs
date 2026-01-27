namespace Domain.Entities.DataBase
{
    public class UserTradeCode : BaseEntity
    {
        public int UserId { get; set; }
        public int TradeCodeId { get; set; }

        public required User User { get; set; }
        public required TradeCode TradeCode { get; set; }
    }
}
