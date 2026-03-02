namespace Domain.Entities.DataBase
{
    public class UserTradeNote : BaseEntity
    {
        public int UserId { get; set; }
        public int TradeCodeId { get; set; }

        public string? NoteText { get; set; }

        public required User User { get; set; }
        public required TradeCode TradeCode { get; set; }
    }
}
