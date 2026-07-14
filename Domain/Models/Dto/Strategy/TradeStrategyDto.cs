namespace Domain.Models.Dto.Strategy
{
    public class TradeStrategyDto
    {
        public required int Id { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public int? Accuracy { get; set; }
        public string? SignalFrequency { get; set; }
        public string? InvestmentHorizon { get; set; }
        public string? LogicDesc { get; set; }
        public string? UseDesc { get; set; }
        public string? LimitDesc { get; set; }
    }
}