using Application.Interfaces.Repositories.Database;
using Domain.Notes.Entities;
using Domain.Notes.Enums;
using FluentValidation;
using MediatR;

namespace Application.Notes.Commands;

public record UpdateUserNoteCommand(int RelatedId, NoteType NoteType, int UserId, string NoteText) : IRequest;

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
    public async Task Handle(UpdateUserNoteCommand request, CancellationToken cancellationToken)
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

        existingNote.UpdateText(request.NoteText);
        noteRepository.Update(existingNote);
        await noteRepository.SaveChangesAsync(cancellationToken);
    }
}
