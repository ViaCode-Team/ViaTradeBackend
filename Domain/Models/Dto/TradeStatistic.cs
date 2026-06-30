using System.ComponentModel.DataAnnotations;

namespace Domain.Models.Dto
{
    public class TradeStatistic
    {
        [Required]
        public int TotalTrades { get; set; }
        [Required]
        public int WinTrades { get; set; }
        [Required]
        public int LoseTrades { get; set; }
    }
}
