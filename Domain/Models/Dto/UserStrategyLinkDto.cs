using System.ComponentModel.DataAnnotations;

namespace Domain.Models.Dto
{
    public class UserStrategyLinkDto
    {
        [Required]
        public required int UserId { get; set; }
        [Required]
        public required int TradeStrategyId { get; set; }
    }
}