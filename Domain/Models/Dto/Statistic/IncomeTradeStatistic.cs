using System.ComponentModel.DataAnnotations;

namespace Domain.Models.Dto.Statistic
{
    public class IncomeTradeStatistic
    {
        [Required]
        public decimal TotalIncome { get; set; }

        [Required]
        public decimal AverageIncome { get; set; }
    }
}
