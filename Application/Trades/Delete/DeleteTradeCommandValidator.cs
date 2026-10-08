using FluentValidation;
using ViaTrade.Application.Common.Validation;

namespace ViaTrade.Application.Trades.Delete;

public sealed class DeleteTradeCommandValidator : AbstractValidator<DeleteTradeCommand>
{
	public DeleteTradeCommandValidator()
	{
		RuleFor(request => request.TradeId).PositiveId();
	}
}
