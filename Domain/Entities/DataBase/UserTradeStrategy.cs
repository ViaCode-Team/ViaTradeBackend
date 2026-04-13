using System.ComponentModel.DataAnnotations;

namespace Domain.Entities.DataBase
{
    public class UserTradeStrategy : BaseEntity
    {
        [Required]
        public required int UserId { get; set; }
        [Required]
        public required int TradeStrategyId { get; set; }

        public User? User { get; set; }
        public TradeStrategy? TradeStrategy { get; set; }
    }
}
