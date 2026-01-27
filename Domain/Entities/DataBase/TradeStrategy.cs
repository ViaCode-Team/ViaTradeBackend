using System.ComponentModel.DataAnnotations;

namespace Domain.Entities.DataBase
{
    public class TradeStrategy : BaseEntity
    {
        [Required]
        [MaxLength(128)]
        public required string Name { get; set; }

        [MaxLength(512)]
        public string? Description { get; set; }

        public ICollection<UserTradeStrategy>? UserTradeStrategies { get; set; }
    }
}
