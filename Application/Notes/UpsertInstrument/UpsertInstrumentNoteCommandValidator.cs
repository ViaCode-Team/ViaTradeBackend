using FluentValidation;
using ViaTrade.Application.Common.Validation;

namespace ViaTrade.Application.Notes.UpsertInstrument;

public sealed class UpsertInstrumentNoteCommandValidator : AbstractValidator<UpsertInstrumentNoteCommand>
{
	public UpsertInstrumentNoteCommandValidator()
	{
		RuleFor(request => request.InstrumentId).PositiveId();
		RuleFor(request => request.Text).RequiredText(1, 1024);
	}
}
