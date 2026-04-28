using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

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

        [JsonIgnore]
        public User? User { get; set; }
        [JsonIgnore]
        public TradeCode? TradeCode { get; set; }
        [JsonIgnore]
        public TradeStrategy? TradeStrategy { get; set; }
    }
}
