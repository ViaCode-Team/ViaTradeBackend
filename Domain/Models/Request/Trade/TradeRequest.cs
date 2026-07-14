using Domain.Entities.DataBase;
using System.ComponentModel.DataAnnotations;

namespace ViaTradeBackend.Models.Trade
{
    public class TradeRequest
    {
        [Required]
        public DateTime DateOpen { get; set; }

        public DateTime? DateClose { get; set; }

        [Required]
        public double TradeOpen { get; set; }

        public double? TradeClose { get; set; }

        public required TradeSignal TradeSignal { get; set; }

        [Range(0, int.MaxValue)]
        public int Count { get; set; }

        [Required]
        public int TradeTypeId { get; set; }

        [Required]
        public int TradeCodeId { get; set; }
    }
}
