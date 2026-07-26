using System.ComponentModel.DataAnnotations;
using ViaTradeBackend.Contracts.Instruments;
using ViaTradeBackend.Contracts.Strategies;

namespace ViaTradeBackend.Contracts.Notes;

public record NoteResponse(
	int Id,
	[StringLength(1024)] string NoteText,
	int UserId,
	InstrumentBriefResponse? Instrument,
	StrategyBriefResponse? Strategy
);
