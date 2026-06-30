using System.ComponentModel.DataAnnotations;

namespace Domain.Models.Dto.Statistic
{
    public class SignalStatistic
    {
        [Required]
        public int TotalSignals { get; set; }

        [Required]
        public int BuySignals { get; set; }

        [Required]
        public int SellSignals { get; set; }
    }
}
