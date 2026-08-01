namespace Application.Notes.Models;

public record NoteDto(int Id, string Text, int UserId, InstrumentBriefDto? Instrument, StrategyBriefDto? Strategy);

public record InstrumentBriefDto(int Id, string Symbol, string? Name);

public record StrategyBriefDto(int Id, string Name, string? Description);

public record NoteProjectionDto(
	int Id,
	string Text,
	int UserId,
	int? InstrumentId,
	string? InstrumentTicker,
	string? InstrumentName,
	int? StrategyId,
	string? StrategyName,
	string? StrategyDescription
);
