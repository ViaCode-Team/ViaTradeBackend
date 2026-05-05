using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Domain.Entities.DataBase
{
    public class UserTradeStrategy : BaseEntity
    {
        [Required]
        public required int UserId { get; set; }
        [Required]
        public required int TradeStrategyId { get; set; }

        [JsonIgnore]
        public User? User { get; set; }
        [JsonIgnore]
        public TradeStrategy? TradeStrategy { get; set; }
    }
}
