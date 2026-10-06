using ViaTrade.Application.Common.Validation;

namespace ViaTrade.Application.Users.UpdateLastLogin;

public sealed class UpdateLastLoginCommandValidator : UserRequestValidator<UpdateLastLoginCommand>
{
	public UpdateLastLoginCommandValidator()
		: base(request => request.UserId) { }
}
