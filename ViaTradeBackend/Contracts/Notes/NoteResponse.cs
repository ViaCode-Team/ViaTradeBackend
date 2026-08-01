using System.ComponentModel.DataAnnotations;
using ViaTradeBackend.Contracts.Instruments;
using ViaTradeBackend.Contracts.Strategies;

namespace ViaTradeBackend.Contracts.Notes;

public record NoteResponse(
	[Range(1, int.MaxValue)] int Id,
	[StringLength(1024, MinimumLength = 1)] string Text,
	[Range(1, int.MaxValue)] int UserId,
	InstrumentBriefResponse? Instrument,
	StrategyBriefResponse? Strategy
);
