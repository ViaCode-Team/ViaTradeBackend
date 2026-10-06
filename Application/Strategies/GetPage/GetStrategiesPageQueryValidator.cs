using ViaTrade.Application.Common.Validation;
using ViaTrade.Application.Strategies.Common;

namespace ViaTrade.Application.Strategies.GetPage;

public sealed class GetStrategiesPageQueryValidator : UserRequestValidator<GetStrategiesPageQuery>
{
	public GetStrategiesPageQueryValidator()
		: base(request => request.UserId)
	{
		RuleFor(request => request.PageOptions).ValidPageOptions();
		RuleFor(request => request.StrategySearch).ValidSearch();
		RuleFor(request => request.StrategySort).RequiredValid(new SortValidator<StrategySort, StrategySortField>());
	}
}
