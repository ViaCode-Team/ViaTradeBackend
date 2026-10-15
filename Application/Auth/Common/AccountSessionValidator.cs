using ViaTrade.Application.Common.Abstractions.Repositories;
using ViaTrade.Domain.Entities;

namespace ViaTrade.Application.Auth.Common;

public sealed class AccountSessionValidator(IReadRepository<User> users)
{
	public Task<bool> IsCurrentAsync(SessionData session, CancellationToken ct = default) =>
		users.AnyAsync(
			user =>
				user.Id == session.UserId
				&& user.AuthorizationVersion == session.AuthorizationVersion
				&& user.Email != null
				&& user.EmailConfirmedAt != null,
			ct
		);
}
