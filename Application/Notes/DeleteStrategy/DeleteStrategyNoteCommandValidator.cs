using FluentValidation;
using ViaTrade.Application.Common.Validation;

namespace ViaTrade.Application.Notes.DeleteStrategy;

public sealed class DeleteStrategyNoteCommandValidator : AbstractValidator<DeleteStrategyNoteCommand>
{
	public DeleteStrategyNoteCommandValidator()
	{
		RuleFor(request => request.StrategyId).PositiveId();
	}
}
