using Application.Users.Interfaces;
using Domain.Users.Entities;
using MediatR;

namespace Application.Users.Queries;

public record GetUserByLoginQuery(string Login) : IRequest<User?>;

public class GetUserByLoginQueryHandler(IUserRepository userRepository)
	: IRequestHandler<GetUserByLoginQuery, User?>
{
	private readonly IUserRepository _userRepository = userRepository;

	public async Task<User?> Handle(GetUserByLoginQuery request, CancellationToken cancellationToken)
	{
		return await _userRepository.GetByLoginAsync(request.Login, cancellationToken);
	}
}
