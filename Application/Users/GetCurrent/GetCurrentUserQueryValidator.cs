using ViaTrade.Application.Common.Validation;

namespace ViaTrade.Application.Users.GetCurrent;

public sealed class GetCurrentUserQueryValidator : UserRequestValidator<GetCurrentUserQuery>
{
	public GetCurrentUserQueryValidator()
		: base(request => request.UserId) { }
}
