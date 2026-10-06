using ViaTrade.Application.Common.Validation;
using ViaTrade.Application.Instruments.Common;

namespace ViaTrade.Application.Strategies.GetInstrumentsPage;

public sealed class GetStrategyInstrumentsPageQueryValidator : UserRequestValidator<GetStrategyInstrumentsPageQuery>
{
	public GetStrategyInstrumentsPageQueryValidator()
		: base(request => request.UserId)
	{
		RuleFor(request => request.StrategyId).PositiveId();
		RuleFor(request => request.InstrumentFilter).RequiredValid(new StrategyInstrumentFilterValidator());
		RuleFor(request => request.PageOptions).ValidPageOptions();
		RuleFor(request => request.InstrumentSort)
			.RequiredValid(new SortValidator<InstrumentSort, InstrumentSortField>());
	}
}
