using Domain.Common;
using Domain.Strategies.Entities;
using Domain.TradeCodes.Entities;
using Domain.Users.Entities;

namespace Domain.Notes.Entities;

public sealed class Note : AggregateRoot<int>
{
	public int UserId { get; private set; }

	public string NoteText { get; private set; }

	public int? TradeCodeId { get; private set; }

	public int? TradeStrategyId { get; private set; }
	public User? User { get; private set; }
	public TradeCode? TradeCode { get; private set; }
	public TradeStrategy? TradeStrategy { get; private set; }

	private Note() { }

	public Note(int userId, string noteText, int? tradeCodeId = null, int? tradeStrategyId = null)
	{
		if (tradeCodeId.HasValue && tradeStrategyId.HasValue)
			throw new ArgumentException("Note cannot belong to both TradeCode and TradeStrategy");

		if (!tradeCodeId.HasValue && !tradeStrategyId.HasValue)
			throw new ArgumentException("Note must belong to either TradeCode or TradeStrategy");

		if (string.IsNullOrEmpty(noteText))
			throw new ArgumentNullException(nameof(noteText));

		UserId = userId;
		NoteText = noteText;
		TradeCodeId = tradeCodeId;
		TradeStrategyId = tradeStrategyId;
	}

	public void UpdateText(string newText)
	{
		if (string.IsNullOrEmpty(newText))
			throw new ArgumentNullException(nameof(newText));

		NoteText = newText;
	}
}
