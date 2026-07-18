using Application.Notes.Interfaces;
using Domain.Notes.Entities;
using Domain.Notes.Enums;
using FluentValidation;
using MediatR;

namespace Application.Notes.Commands;

public record AddUserNoteCommand(int RelatedId, NoteType NoteType, int UserId, string NoteText) : IRequest;

public class AddUserNoteValidator : AbstractValidator<AddUserNoteCommand>
{
	public AddUserNoteValidator()
	{
		RuleFor(x => x.NoteText).NotEmpty().MaximumLength(1024);
		RuleFor(x => x.UserId).GreaterThan(0);
		RuleFor(x => x.RelatedId).GreaterThan(0);
	}
}

public class AddUserNoteCommandHandler(INoteRepository noteRepository) : IRequestHandler<AddUserNoteCommand>
{
	public async Task Handle(AddUserNoteCommand request, CancellationToken cancellationToken)
	{
		var existingNotes = await noteRepository.FindAsync(x =>
				x.UserId == request.UserId &&
				(request.NoteType == NoteType.TradeCodeNote ? x.TradeCodeId == request.RelatedId : x.TradeStrategyId == request.RelatedId),
			cancellationToken);

		var existingNote = existingNotes.FirstOrDefault();

		if (existingNote != null)
		{
			throw new Exception("Note already exists. Use update.");
		}

		var note = new Note(
			request.UserId,
			request.NoteText,
			request.NoteType == NoteType.TradeCodeNote ? request.RelatedId : null,
			request.NoteType == NoteType.TradeStrategyNote ? request.RelatedId : null
		);

		await noteRepository.AddAsync(note, cancellationToken);
	}
}
