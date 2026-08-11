namespace Application.Notes.Models;

public sealed class NoteSearchFilter
{
	public string? Text { get; set; }

	public string? InstrumentSymbol { get; set; }
	public string? InstrumentDescription { get; set; }
	public string? StrategyName { get; set; }
}
