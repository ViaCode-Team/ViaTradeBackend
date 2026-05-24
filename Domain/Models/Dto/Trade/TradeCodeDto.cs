using System.ComponentModel.DataAnnotations;

namespace Domain.Models.Dto.Trade
{
    public class TradeCodeDto
    {
        [Required]
        public required int Id { get; set; }
        [Required]
        public required string ExchangeId { get; set; }
        public string? Description { get; set; }
    }
}