namespace Application.Statistics.Models;

public class NoteStatisticReadModel
{
	public required int TotalNotes { get; set; }

	public required int StockNotes { get; set; }

	public required int StrategyNotes { get; set; }
}
