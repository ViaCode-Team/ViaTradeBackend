namespace Domain.Entities.DataBase
{
    public class UserStrategyNote : BaseEntity
    {
        public int UserId { get; set; }
        public int StratageId { get; set; }

        public string? NoteText { get; set; }

        public required User User { get; set; }
        public required TradeStrategy Trade { get; set; }

    }
}
