namespace Domain.Entities.DataBase
{
    public class Trade : BaseEntity
    {
        public DateTime DateOpen { get; set; }
        public DateTime? DateClose { get; set; }

        public double TradeOpen { get; set; }
        public double? TradeClose { get; set; }

        public double? NetIncome { get; set; }
        public int Count { get; set; }
        public int Price { get; set; }

        public int TradeTypeId { get; set; }
        public int TradeCodeId { get; set; }
        public int UserId { get; set; }

        public required TradeType TradeType { get; set; }
        public required TradeCode TradeCode { get; set; }
        public required User User { get; set; }
    }
}
