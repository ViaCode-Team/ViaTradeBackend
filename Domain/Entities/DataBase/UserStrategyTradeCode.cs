using System.ComponentModel.DataAnnotations;

namespace Domain.Entities.DataBase
{
    public class UserStrategyTradeCode : BaseEntity
    {
        [Required]
        public required int UserId { get; set; }
        [Required]
        public required int TradeCodeId { get; set; }
        [Required]
        public required int StrategyId { get; set; }
    }
}
