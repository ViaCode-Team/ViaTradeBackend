using FluentValidation;
using ViaTrade.Application.Common.Validation;

namespace ViaTrade.Application.Trades.Create;

public sealed class CreateTradeCommandValidator : AbstractValidator<CreateTradeCommand>
{
	public CreateTradeCommandValidator()
	{
		RuleFor(request => request.OpenPrice).InclusiveBetween(double.Epsilon, double.MaxValue);
		RuleFor(request => request.ClosePrice).InclusiveBetween(double.Epsilon, double.MaxValue);
		RuleFor(request => request.Signal).IsInEnum();
		RuleFor(request => request.Quantity).PositiveId();
		RuleFor(request => request.TradeTypeId).PositiveId();
		RuleFor(request => request.InstrumentId).PositiveId();
		RuleFor(request => request.ClosedAt)
			.Must((request, closedAt) => !closedAt.HasValue || closedAt.Value >= request.OpenedAt)
			.WithMessage("closedAt must be greater than or equal to openedAt.");
		RuleFor(request => request.ClosePrice)
			.Must((request, closePrice) => request.ClosedAt.HasValue == closePrice.HasValue)
			.WithMessage("closedAt and closePrice must either both be specified or both be omitted.");
	}
}
