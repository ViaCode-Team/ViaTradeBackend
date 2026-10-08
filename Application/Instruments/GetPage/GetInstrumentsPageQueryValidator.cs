using FluentValidation;
using ViaTrade.Application.Common.Validation;
using ViaTrade.Application.Instruments.Common;

namespace ViaTrade.Application.Instruments.GetPage;

public sealed class GetInstrumentsPageQueryValidator : AbstractValidator<GetInstrumentsPageQuery>
{
	public GetInstrumentsPageQueryValidator()
	{
		RuleFor(request => request.PageOptions).ValidPageOptions();
		RuleFor(request => request.InstrumentSearch).ValidSearch();
		RuleFor(request => request.InstrumentSort)
			.RequiredValid(new SortValidator<InstrumentSort, InstrumentSortField>());
	}
}
