using Mediator;
using ViaTrade.Application.Common.Abstractions;
using ViaTrade.Application.Common.Abstractions.Repositories;
using ViaTrade.Application.Common.Exceptions;
using ViaTrade.Application.Notes.Common;
using ViaTrade.Domain.Entities;

namespace ViaTrade.Application.Notes.GetStrategy;

public sealed class GetStrategyNoteHandler(
	IUserContext userContext,
	IReadRepository<Strategy> strategyRepository,
	IReadRepository<Note> noteRepository
) : IQueryHandler<GetStrategyNoteQuery, NoteResult>
{
	public async ValueTask<NoteResult> Handle(GetStrategyNoteQuery query, CancellationToken ct)
	{
		var strategyExists = await strategyRepository.AnyAsync(strategy => strategy.Id == query.StrategyId, ct);
		if (!strategyExists)
			throw new NotFoundException("Strategy not found.", "strategy_not_found");

		var note = await noteRepository.FirstOrDefaultAsync(
			note => note.UserId == userContext.UserId && note.StrategyId == query.StrategyId,
			NoteResult.Projection,
			ct
		);
		if (note == null)
			throw new NotFoundException("Note not found.", "note_not_found");

		return note;
	}
}
