using ViaTrade.Application.Common.Validation;

namespace ViaTrade.Application.Trades.Delete;

public sealed class DeleteTradeCommandValidator : UserRequestValidator<DeleteTradeCommand>
{
	public DeleteTradeCommandValidator()
		: base(request => request.UserId)
	{
		RuleFor(request => request.TradeId).PositiveId();
	}
}
