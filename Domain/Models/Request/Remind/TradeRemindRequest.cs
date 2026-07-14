using System.ComponentModel.DataAnnotations;

namespace Domain.Models.Dto
{
    public class TradeRemindRequest
    {
        [StringLength(1024)]
        public required string TextRemind { get; set; }

        public required DateTime DateTime { get; set; }
    }
}