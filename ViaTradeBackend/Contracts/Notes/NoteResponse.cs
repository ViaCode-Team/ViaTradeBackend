using System.ComponentModel.DataAnnotations;

namespace ViaTradeBackend.Contracts.Notes;

public record NoteResponse(
	int Id,
	[StringLength(1024)] string NoteText,
	int UserId,
	int? TradeCodeId,
	int? TradeStrategyId
);
