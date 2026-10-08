using FluentValidation;
using ViaTrade.Application.Common.Validation;
using ViaTrade.Application.Strategies.Common;

namespace ViaTrade.Application.Strategies.GetPage;

public sealed class GetStrategiesPageQueryValidator : AbstractValidator<GetStrategiesPageQuery>
{
	public GetStrategiesPageQueryValidator()
	{
		RuleFor(request => request.PageOptions).ValidPageOptions();
		RuleFor(request => request.StrategySearch).ValidSearch();
		RuleFor(request => request.StrategySort).RequiredValid(new SortValidator<StrategySort, StrategySortField>());
	}
}
