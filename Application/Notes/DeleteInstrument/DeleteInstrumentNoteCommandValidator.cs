using FluentValidation;
using ViaTrade.Application.Common.Validation;

namespace ViaTrade.Application.Notes.DeleteInstrument;

public sealed class DeleteInstrumentNoteCommandValidator : AbstractValidator<DeleteInstrumentNoteCommand>
{
	public DeleteInstrumentNoteCommandValidator()
	{
		RuleFor(request => request.InstrumentId).PositiveId();
	}
}
