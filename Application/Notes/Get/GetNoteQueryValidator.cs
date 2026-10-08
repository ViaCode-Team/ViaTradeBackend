using FluentValidation;
using ViaTrade.Application.Common.Validation;

namespace ViaTrade.Application.Notes.Get;

public sealed class GetNoteQueryValidator : AbstractValidator<GetNoteQuery>
{
	public GetNoteQueryValidator()
	{
		RuleFor(request => request.NoteId).PositiveId();
	}
}
