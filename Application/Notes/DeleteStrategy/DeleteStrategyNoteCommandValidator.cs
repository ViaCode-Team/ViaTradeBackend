using ViaTrade.Application.Common.Validation;

namespace ViaTrade.Application.Notes.DeleteStrategy;

public sealed class DeleteStrategyNoteCommandValidator : UserRequestValidator<DeleteStrategyNoteCommand>
{
	public DeleteStrategyNoteCommandValidator()
		: base(request => request.UserId)
	{
		RuleFor(request => request.StrategyId).PositiveId();
	}
}
