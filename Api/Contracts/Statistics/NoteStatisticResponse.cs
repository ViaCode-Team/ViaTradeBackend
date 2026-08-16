using System.ComponentModel.DataAnnotations;

namespace ViaTrade.Api.Contracts.Statistics;

public record NoteStatisticResponse(
	[Range(0, int.MaxValue)] int TotalNotes,
	[Range(0, int.MaxValue)] int InstrumentNotes,
	[Range(0, int.MaxValue)] int StrategyNotes
);
