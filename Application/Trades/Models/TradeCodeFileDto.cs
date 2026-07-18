namespace Application.Trades.Models;

public class TradeCodeFileDto
{
	public required int Id { get; set; }

	public required string ExchangeId { get; set; }

	public required string TimeFrame { get; set; }

	public required DateTime StartDate { get; set; }

	public required DateTime EndDate { get; set; }
}
