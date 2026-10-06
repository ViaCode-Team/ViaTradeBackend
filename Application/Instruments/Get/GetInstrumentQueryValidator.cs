using FluentValidation;
using ViaTrade.Application.Common.Validation;

namespace ViaTrade.Application.Instruments.Get;

public sealed class GetInstrumentQueryValidator : AbstractValidator<GetInstrumentQuery>
{
	public GetInstrumentQueryValidator()
	{
		RuleFor(request => request.InstrumentId).PositiveId();
	}
}
