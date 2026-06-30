using System.ComponentModel.DataAnnotations;

namespace Domain.Models.Dto.Statistic
{
    public class WinrateTradeStatistic
    {
        [Required]
        public float TotalWinrate { get; set; }

        [Required]
        public float ProfitFactor { get; set; }
    }
}
