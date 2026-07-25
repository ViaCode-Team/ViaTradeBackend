namespace Application.Notes.Models;

public record NoteDto(int Id, string NoteText, int UserId, TradeCodeBriefDto? TradeCode, StrategyBriefDto? Strategy);

public record TradeCodeBriefDto(int Id, string Ticker, string? Name);

public record StrategyBriefDto(int Id, string Name, string? Description);

public record NoteProjectionDto(
	int Id,
	string NoteText,
	int UserId,
	int? TradeCodeId,
	string? TradeCodeTicker,
	string? TradeCodeName,
	int? StrategyId,
	string? StrategyName,
	string? StrategyDescription
);
