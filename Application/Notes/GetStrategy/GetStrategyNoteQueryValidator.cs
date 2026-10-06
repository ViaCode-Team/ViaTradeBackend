using ViaTrade.Application.Common.Validation;

namespace ViaTrade.Application.Notes.GetStrategy;

public sealed class GetStrategyNoteQueryValidator : UserRequestValidator<GetStrategyNoteQuery>
{
	public GetStrategyNoteQueryValidator()
		: base(request => request.UserId)
	{
		RuleFor(request => request.StrategyId).PositiveId();
	}
}
