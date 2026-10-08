using FluentValidation;
using ViaTrade.Application.Common.Validation;

namespace ViaTrade.Application.Auth.GetSessionsPage;

public sealed class GetSessionsPageQueryValidator : AbstractValidator<GetSessionsPageQuery>
{
	public GetSessionsPageQueryValidator()
	{
		RuleFor(request => request.PageOptions).ValidPageOptions();
	}
}
