using Application.Common.Interfaces;
using Application.Notes.Interfaces;
using Domain.Notes.Entities;
using Domain.Notes.Enums;
using MediatR;

namespace Application.Notes.Queries;

public record GetUserNoteByPropQuery(int RelatedId, int UserId, NoteType NoteType) : IQuery<Note>;

public class GetUserNoteByPropQueryHandler(INoteRepository noteRepository) : IRequestHandler<GetUserNoteByPropQuery, Note>
{
	public async Task<Note> Handle(GetUserNoteByPropQuery request, CancellationToken cancellationToken)
	{
		var existingNotes = await noteRepository.FindAsync(x =>
				x.UserId == request.UserId &&
				(request.NoteType == NoteType.TradeCodeNote ? x.TradeCodeId == request.RelatedId : x.TradeStrategyId == request.RelatedId),
			cancellationToken);

		var existingNote = existingNotes.FirstOrDefault();

		if (existingNote == null)
		{
			throw new Exception("Note not found.");
		}

		return existingNote;
	}
}
