namespace Application.Contracts.Dto.Statistic;

public class NoteStatisticDto
{
	public required int TotalNotes { get; set; }

	public required int StockNotes { get; set; }

	public required int StrategyNotes { get; set; }
}
