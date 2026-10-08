using FluentValidation;
using ViaTrade.Application.Common.Validation;

namespace ViaTrade.Application.Notes.UpsertStrategy;

public sealed class UpsertStrategyNoteCommandValidator : AbstractValidator<UpsertStrategyNoteCommand>
{
	public UpsertStrategyNoteCommandValidator()
	{
		RuleFor(request => request.StrategyId).PositiveId();
		RuleFor(request => request.Text).RequiredText(1, 1024);
	}
}
