using ViaTrade.Application.Common.Abstractions;
using ViaTrade.Application.Common.Abstractions.Repositories;
using ViaTrade.Application.Common.Exceptions;
using ViaTrade.Domain.Entities;

namespace ViaTrade.Application.Users.GetCurrent;

public sealed class GetCurrentUserHandler(IReadRepository<User> userRepository)
	: IQueryHandler<GetCurrentUserQuery, CurrentUserResult>
{
	public async Task<CurrentUserResult> HandleAsync(GetCurrentUserQuery query, CancellationToken ct = default)
	{
		var user = await userRepository.FirstOrDefaultAsync(
			user => user.Id == query.UserId,
			CurrentUserResult.Projection,
			ct
		);
		if (user == null)
			throw new NotFoundException("User not found.", "user_not_found");

		return user;
	}
}
