using System.ComponentModel.DataAnnotations;

namespace Domain.Models.Dto
{
    public class TradeStrategyDto
    {
        [Required]
        public required int Id { get; set; }
        [Required]
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