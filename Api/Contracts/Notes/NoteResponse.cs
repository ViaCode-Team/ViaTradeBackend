using System.ComponentModel.DataAnnotations;
using ViaTrade.Api.Contracts.Instruments;
using ViaTrade.Api.Contracts.Strategies;

namespace ViaTrade.Api.Contracts.Notes;

public record NoteResponse(
	[Range(1, int.MaxValue)] int Id,
	[StringLength(1024, MinimumLength = 1)] string Text,
	[Range(1, int.MaxValue)] int UserId,
	InstrumentBriefResponse? Instrument,
	StrategyBriefResponse? Strategy
);
