using FluentValidation;
using ViaTrade.Application.Common.Validation;

namespace ViaTrade.Application.Notes.GetInstrument;

public sealed class GetInstrumentNoteQueryValidator : AbstractValidator<GetInstrumentNoteQuery>
{
	public GetInstrumentNoteQueryValidator()
	{
		RuleFor(request => request.InstrumentId).PositiveId();
	}
}
