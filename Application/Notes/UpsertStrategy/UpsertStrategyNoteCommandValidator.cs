using ViaTrade.Application.Common.Validation;

namespace ViaTrade.Application.Notes.UpsertStrategy;

public sealed class UpsertStrategyNoteCommandValidator : UserRequestValidator<UpsertStrategyNoteCommand>
{
	public UpsertStrategyNoteCommandValidator()
		: base(request => request.UserId)
	{
		RuleFor(request => request.StrategyId).PositiveId();
		RuleFor(request => request.Text).RequiredText(1, 1024);
	}
}
