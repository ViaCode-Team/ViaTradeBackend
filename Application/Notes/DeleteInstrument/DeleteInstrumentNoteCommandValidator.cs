using ViaTrade.Application.Common.Validation;

namespace ViaTrade.Application.Notes.DeleteInstrument;

public sealed class DeleteInstrumentNoteCommandValidator : UserRequestValidator<DeleteInstrumentNoteCommand>
{
	public DeleteInstrumentNoteCommandValidator()
		: base(request => request.UserId)
	{
		RuleFor(request => request.InstrumentId).PositiveId();
	}
}
