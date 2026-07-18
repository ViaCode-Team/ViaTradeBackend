using Application.Notes.Interfaces;
using Domain.Notes.Enums;
using MediatR;

namespace Application.Notes.Commands;

public record DeleteUserNoteCommand(int RelatedId, int UserId, NoteType NoteType) : IRequest;

public class DeleteUserNoteCommandHandler(INoteRepository noteRepository) : IRequestHandler<DeleteUserNoteCommand>
{
	public async Task Handle(DeleteUserNoteCommand request, CancellationToken cancellationToken)
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

		noteRepository.Remove(existingNote);
		await noteRepository.SaveChangesAsync(cancellationToken);
	}
}
