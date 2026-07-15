using System.ComponentModel.DataAnnotations;

namespace Domain.Models.Dto.Trade;

public class TradeTypeDto
{
	[MaxLength(128)]
	public required string Name { get; set; }
}
