namespace Domain.Models.Dto
{
    public record TradeStrategyDto(string Name, string? Description, int? Accuracy,
        string? SignalFrequency, string? InvestmentHorizon, string? LogicDesc, string? UseDesc, 
        string? LimitDesc);
}
