using Domain.Strategies.Entities;
using Domain.Notes.Entities;
using System.ComponentModel.DataAnnotations;
using Domain.Common;
using Domain.Entities.DataBase; // Temporary for User, TradeCode, TradeStrategy

namespace Domain.Notes.Entities;

public class Note : AggregateRoot
{
    public int UserId { get; private set; }

    [StringLength(1024)]
    public string NoteText { get; private set; }

    public int? TradeCodeId { get; private set; }

    public int? TradeStrategyId { get; private set; }

    // Navigation properties for EF Core
    public User? User { get; private set; }
    public TradeCode? TradeCode { get; private set; }
    public TradeStrategy? TradeStrategy { get; private set; }

    private Note() { } // For EF Core

    public Note(int userId, string noteText, int? tradeCodeId = null, int? tradeStrategyId = null)
    {
        if (tradeCodeId.HasValue && tradeStrategyId.HasValue)
            throw new ArgumentException("Note cannot belong to both TradeCode and TradeStrategy");

        if (!tradeCodeId.HasValue && !tradeStrategyId.HasValue)
            throw new ArgumentException("Note must belong to either TradeCode or TradeStrategy");

        UserId = userId;
        NoteText = noteText ?? throw new ArgumentNullException(nameof(noteText));
        TradeCodeId = tradeCodeId;
        TradeStrategyId = tradeStrategyId;
    }

    public void UpdateText(string newText)
    {
        NoteText = newText ?? throw new ArgumentNullException(nameof(newText));
    }
}
