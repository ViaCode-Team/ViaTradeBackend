using System.ComponentModel.DataAnnotations;

namespace Domain.Entities.DataBase
{

    public class TradeCode : BaseEntity
    {
        [Required]
        [MaxLength(128)]
        public required string ExchangeId { get; set; }

        [MaxLength(512)]
        public string? Description { get; set; }

        public ICollection<Trade>? Trades { get; set; }
        public ICollection<UserTradeCode>? UserTradeCodes { get; set; }
    }
}
