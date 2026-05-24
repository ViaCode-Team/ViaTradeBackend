using System.ComponentModel.DataAnnotations;

namespace Domain.Models.Dto.Strategy
{
    public class UserTradeStrategyDto
    {
        [Required]
        public required int Id { get; set; }
        [Required]
        public required int UserId { get; set; }
        [Required]
        public required int TradeStrategyId { get; set; }
    }
}