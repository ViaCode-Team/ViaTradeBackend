using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Domain.Entities.DataBase
{
    public class Trade : BaseEntity
    {
        public DateTime DateOpen { get; set; }
        public DateTime? DateClose { get; set; }

        public double TradeOpen { get; set; }
        public double? TradeClose { get; set; }

        public double? NetIncome { get; set; }
        public int Count { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        public int TradeTypeId { get; set; }
        public int TradeCodeId { get; set; }
        public int UserId { get; set; }

        public required TradeSignal TradeSignal { get; set; }

        [JsonIgnore]
        public TradeType? TradeType { get; set; }
        [JsonIgnore]
        public TradeCode? TradeCode { get; set; }
        [JsonIgnore]
        public User? User { get; set; }
    }

    public enum TradeSignal
    {
        HOLD = 0,
        BUY = 1,
        SELL = -1,
    }
}
