namespace Domain.Models.Dto.Trade;

public class TradeCodeDto
{
	public required int Id { get; set; }

	public required string ExchangeId { get; set; }

	public string? Description { get; set; }
}
