namespace ViaTrade.Domain.Models.Trade;

public class InstrumentFile
{
	public required string Ticker { get; set; }

	public required string TimeFrame { get; set; }

	public required DateTime StartDate { get; set; }

	public required DateTime EndDate { get; set; }
}
