namespace ViaTrade.Application.Trades.Models;

public class InstrumentFileDto
{
	public required int Id { get; set; }

	public required string Symbol { get; set; }

	public required string TimeFrame { get; set; }

	public required DateTime StartDate { get; set; }

	public required DateTime EndDate { get; set; }
}
