using Application.Common.Interfaces;
using Application.Notes.Interfaces;
using Domain.Notes.Enums;
using FluentValidation;
using MediatR;

namespace Application.Notes.Commands;

public record UpdateUserNoteCommand(int RelatedId, NoteType NoteType, int UserId, string NoteText) : ICommand;

public class UpdateUserNoteValidator : AbstractValidator<UpdateUserNoteCommand>
{
	public UpdateUserNoteValidator()
	{
		RuleFor(x => x.NoteText).NotEmpty().MaximumLength(1024);
		RuleFor(x => x.UserId).GreaterThan(0);
		RuleFor(x => x.RelatedId).GreaterThan(0);
	}
}

public class UpdateUserNoteCommandHandler(INoteRepository noteRepository) : IRequestHandler<UpdateUserNoteCommand>
{
	public async Task Handle(UpdateUserNoteCommand request, CancellationToken ct)
	{
		try
		{
			await noteRepository.UpdateUserNoteAsync(
				request.RelatedId,
				request.NoteType,
				request.UserId,
				request.NoteText,
				ct);
		}
		catch (KeyNotFoundException)
		{
			throw new Exception("Note not found.");
		}
	}
}
