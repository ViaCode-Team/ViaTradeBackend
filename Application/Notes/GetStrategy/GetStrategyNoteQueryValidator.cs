using FluentValidation;
using ViaTrade.Application.Common.Validation;

namespace ViaTrade.Application.Notes.GetStrategy;

public sealed class GetStrategyNoteQueryValidator : AbstractValidator<GetStrategyNoteQuery>
{
	public GetStrategyNoteQueryValidator()
	{
		RuleFor(request => request.StrategyId).PositiveId();
	}
}
