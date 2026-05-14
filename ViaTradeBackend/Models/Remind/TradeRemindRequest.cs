using System.ComponentModel.DataAnnotations;

namespace Domain.Models.Dto
{
    public class TradeRemindRequest
    {
        [Required]
        [StringLength(1024)]
        public required string TextRemind { get; set; }

        [Required]
        public required DateTime DateTime { get; set; }
    }
}