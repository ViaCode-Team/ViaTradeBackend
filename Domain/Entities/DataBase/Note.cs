using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Domain.Entities.DataBase
{
    public class Note : BaseEntity
    {
        public int UserId { get; set; }

        [Required]
        [StringLength(1024)]
        public required string NoteText { get; set; }

        public int TypeId { get; set; }

        public int? TradeCodeId { get; set; }

        public int? TradeStrategyId { get; set; }

        [JsonIgnore]
        public User? User { get; set; }
        [JsonIgnore]
        public NoteType? NoteType { get; set; }
        [JsonIgnore]
        public TradeCode? TradeCode { get; set; }
        [JsonIgnore]
        public TradeStrategy? TradeStrategy { get; set; }
    }
}
