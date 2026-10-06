using ViaTrade.Application.Common.Validation;

namespace ViaTrade.Application.Auth.GetSessionsPage;

public sealed class GetSessionsPageQueryValidator : UserRequestValidator<GetSessionsPageQuery>
{
	public GetSessionsPageQueryValidator()
		: base(request => request.UserId)
	{
		RuleFor(request => request.PageOptions).ValidPageOptions();
	}
}
