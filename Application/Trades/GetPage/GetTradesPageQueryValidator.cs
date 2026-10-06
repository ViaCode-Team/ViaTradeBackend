using ViaTrade.Application.Common.Validation;

namespace ViaTrade.Application.Trades.GetPage;

public sealed class GetTradesPageQueryValidator : UserRequestValidator<GetTradesPageQuery>
{
	public GetTradesPageQueryValidator()
		: base(request => request.UserId)
	{
		RuleFor(request => request.PageOptions).ValidPageOptions();
		RuleFor(request => request.TradeSearch).ValidSearch();
	}
}
