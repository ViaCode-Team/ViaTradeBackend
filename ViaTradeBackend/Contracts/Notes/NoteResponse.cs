using System.ComponentModel.DataAnnotations;
using ViaTradeBackend.Contracts.Strategies;
using ViaTradeBackend.Contracts.Trades;

namespace ViaTradeBackend.Contracts.Notes;

public record NoteResponse(
	int Id,
	[StringLength(1024)] string NoteText,
	int UserId,
	TradeCodeBriefResponse? TradeCode,
	StrategyBriefResponse? Strategy
);
