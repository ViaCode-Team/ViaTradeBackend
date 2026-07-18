namespace ViaTradeBackend.Contracts.Statistics;

public record NoteStatisticResponse(
	int TotalNotes,
	int StockNotes,
	int StrategyNotes
);

