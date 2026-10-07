using System.Linq.Expressions;
using ViaTrade.Application.Common.Models;
using ViaTrade.Domain.Entities;

namespace ViaTrade.Application.Notes.Common;

public record NoteResult(
	int Id,
	string Text,
	int UserId,
	InstrumentBriefResult? Instrument,
	StrategyBriefResult? Strategy
)
{
	public static Expression<Func<Note, NoteResult>> Projection { get; } =
		note => new NoteResult(
			note.Id,
			note.Text,
			note.UserId,
			note.Instrument == null
				? null
				: new InstrumentBriefResult(note.Instrument.Id, note.Instrument.Ticker, note.Instrument.Description),
			note.Strategy == null
				? null
				: new StrategyBriefResult(
					note.Strategy.Id,
					note.Strategy.Name,
					note.Strategy.DisplayName,
					note.Strategy.Description
				)
		);
}
