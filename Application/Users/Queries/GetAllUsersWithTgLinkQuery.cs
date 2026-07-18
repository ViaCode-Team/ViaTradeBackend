using Application.Interfaces.Repositories.Database;
using Domain.Users.Entities;
using MediatR;

namespace Application.Users.Queries;

public record GetAllUsersWithTgLinkQuery() : IRequest<IEnumerable<User>>;

public class GetAllUsersWithTgLinkQueryHandler(IUserRepository userRepository) 
	: IRequestHandler<GetAllUsersWithTgLinkQuery, IEnumerable<User>>
{
	private readonly IUserRepository _userRepository = userRepository;

	public async Task<IEnumerable<User>> Handle(GetAllUsersWithTgLinkQuery request, CancellationToken cancellationToken)
	{
		return await _userRepository.GetAllWithTgLinkAsync(cancellationToken);
	}
}
