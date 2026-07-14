using System.ComponentModel.DataAnnotations;

namespace ViaTradeBackend.Models.Trade;

public class CreateUserStrategyRequest
{
	[Required]
	public required int StrategyId { get; set; }
}
