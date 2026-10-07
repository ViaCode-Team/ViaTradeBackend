using FluentValidation;
using ViaTrade.Application.Common.Validation;

namespace ViaTrade.Application.Trades.Get;

public sealed class GetTradeQueryValidator : AbstractValidator<GetTradeQuery>
{
	public GetTradeQueryValidator()
	{
		RuleFor(request => request.TradeId).PositiveId();
	}
}
