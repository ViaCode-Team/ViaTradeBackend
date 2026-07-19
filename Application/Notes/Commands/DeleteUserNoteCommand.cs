using Application.Common.Interfaces;
using Application.Notes.Interfaces;
using Domain.Notes.Enums;
using MediatR;

namespace Application.Notes.Commands;

public record DeleteUserNoteCommand(int RelatedId, int UserId, NoteType NoteType) : ICommand;

public class DeleteUserNoteCommandHandler(INoteRepository noteRepository) : IRequestHandler<DeleteUserNoteCommand>
{
	public async Task Handle(DeleteUserNoteCommand request, CancellationToken cancellationToken)
	{
		var rows = await noteRepository.ExecuteDeleteAsync(
			x => x.UserId == request.UserId &&
				 (request.NoteType == NoteType.TradeCodeNote ? x.TradeCodeId == request.RelatedId : x.TradeStrategyId == request.RelatedId),
			cancellationToken);

		if (rows == 0)
			throw new Exception("Note not found.");
	}
}
