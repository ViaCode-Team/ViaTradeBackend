using System.ComponentModel.DataAnnotations;

namespace Domain.Models.Dto.Strategy
{
    public class UserStrategyTradeCodeDto
    {
        [Required]
        public required int UserId { get; set; }
        [Required]
        public required int TradeCodeId { get; set; }
        [Required]
        public required int StrategyId { get; set; }
    }
}