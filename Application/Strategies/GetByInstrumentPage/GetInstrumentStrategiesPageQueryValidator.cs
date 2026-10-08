using FluentValidation;
using ViaTrade.Application.Common.Validation;
using ViaTrade.Application.Strategies.Common;

namespace ViaTrade.Application.Strategies.GetByInstrumentPage;

public sealed class GetInstrumentStrategiesPageQueryValidator : AbstractValidator<GetInstrumentStrategiesPageQuery>
{
	public GetInstrumentStrategiesPageQueryValidator()
	{
		RuleFor(request => request.InstrumentId).PositiveId();
		RuleFor(request => request.PageOptions).ValidPageOptions();
		RuleFor(request => request.StrategySort).RequiredValid(new SortValidator<StrategySort, StrategySortField>());
	}
}
