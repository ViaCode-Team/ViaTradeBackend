using Application.Common.Interfaces;
using Application.Users.Interfaces;
using Domain.Users.Entities;
using MediatR;

namespace Application.Users.Queries;

public record GetAllUsersWithTgLinkQuery() : IQuery<IEnumerable<User>>;

public class GetAllUsersWithTgLinkQueryHandler(IUserRepository userRepository)
	: IRequestHandler<GetAllUsersWithTgLinkQuery, IEnumerable<User>>
{
	public async Task<IEnumerable<User>> Handle(GetAllUsersWithTgLinkQuery request, CancellationToken ct)
	{
		return await userRepository.GetAllWithTgLinkAsync(ct);
	}
}
