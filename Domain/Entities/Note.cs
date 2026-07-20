using Domain.Entities;
using Domain.Strategies.Entities;
using Domain.TradeCodes.Entities;
using Domain.Users.Entities;

namespace Domain.Notes.Entities;

public sealed class Note : BaseEntity<int>
{
	public required int UserId { get; set; }

	public required string NoteText { get; set; }

	public int? TradeCodeId { get; set; }

	public int? TradeStrategyId { get; set; }

	public User? User { get; set; }

	public TradeCode? TradeCode { get; set; }

	public TradeStrategy? TradeStrategy { get; set; }
}
