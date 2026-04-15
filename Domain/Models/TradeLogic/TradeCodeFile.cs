namespace Domain.Models.TradeLogic
{
    public class TradeCodeFile
    {
        public required string TradeCode { get; set; }
        public required string TimeFrame { get; set; }
        public required DateTime StartDate { get; set; }
        public required DateTime EndDate { get; set; }
    }
}
