using ViaTrade.Application.Common.Validation;

namespace ViaTrade.Application.Trades.Get;

public sealed class GetTradeQueryValidator : UserRequestValidator<GetTradeQuery>
{
	public GetTradeQueryValidator()
		: base(request => request.UserId)
	{
		RuleFor(request => request.TradeId).PositiveId();
	}
}
