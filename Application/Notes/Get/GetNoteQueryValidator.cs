using ViaTrade.Application.Common.Validation;

namespace ViaTrade.Application.Notes.Get;

public sealed class GetNoteQueryValidator : UserRequestValidator<GetNoteQuery>
{
	public GetNoteQueryValidator()
		: base(request => request.UserId)
	{
		RuleFor(request => request.NoteId).PositiveId();
	}
}
