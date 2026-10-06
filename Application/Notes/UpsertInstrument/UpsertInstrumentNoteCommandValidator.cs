using ViaTrade.Application.Common.Validation;

namespace ViaTrade.Application.Notes.UpsertInstrument;

public sealed class UpsertInstrumentNoteCommandValidator : UserRequestValidator<UpsertInstrumentNoteCommand>
{
	public UpsertInstrumentNoteCommandValidator()
		: base(request => request.UserId)
	{
		RuleFor(request => request.InstrumentId).PositiveId();
		RuleFor(request => request.Text).RequiredText(1, 1024);
	}
}
