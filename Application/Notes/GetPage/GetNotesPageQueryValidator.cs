using FluentValidation;
using ViaTrade.Application.Common.Validation;

namespace ViaTrade.Application.Notes.GetPage;

public sealed class GetNotesPageQueryValidator : AbstractValidator<GetNotesPageQuery>
{
	public GetNotesPageQueryValidator()
	{
		RuleFor(request => request.PageOptions).ValidPageOptions();
		RuleFor(request => request.NoteSearch).ValidSearch();
	}
}
