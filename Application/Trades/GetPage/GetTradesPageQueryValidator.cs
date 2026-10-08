using FluentValidation;
using ViaTrade.Application.Common.Validation;

namespace ViaTrade.Application.Trades.GetPage;

public sealed class GetTradesPageQueryValidator : AbstractValidator<GetTradesPageQuery>
{
	public GetTradesPageQueryValidator()
	{
		RuleFor(request => request.PageOptions).ValidPageOptions();
		RuleFor(request => request.TradeSearch).ValidSearch();
	}
}
