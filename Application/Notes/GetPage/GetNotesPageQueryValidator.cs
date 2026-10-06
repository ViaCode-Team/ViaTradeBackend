using ViaTrade.Application.Common.Validation;

namespace ViaTrade.Application.Notes.GetPage;

public sealed class GetNotesPageQueryValidator : UserRequestValidator<GetNotesPageQuery>
{
	public GetNotesPageQueryValidator()
		: base(request => request.UserId)
	{
		RuleFor(request => request.PageOptions).ValidPageOptions();
		RuleFor(request => request.NoteSearch).ValidSearch();
	}
}
