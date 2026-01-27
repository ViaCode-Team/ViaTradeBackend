using System.ComponentModel.DataAnnotations;

namespace Domain.Entities.DataBase
{
    public class TradeType : BaseEntity
    {
        [Required]
        [MaxLength(128)]
        public required string Name { get; set; }

        public ICollection<Trade>? Trades { get; set; }
    }
}
