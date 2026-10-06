using ViaTrade.Application.Common.Validation;

namespace ViaTrade.Application.Notes.GetInstrument;

public sealed class GetInstrumentNoteQueryValidator : UserRequestValidator<GetInstrumentNoteQuery>
{
	public GetInstrumentNoteQueryValidator()
		: base(request => request.UserId)
	{
		RuleFor(request => request.InstrumentId).PositiveId();
	}
}
