using Application.Common.Interfaces;
using Application.Users.Interfaces;
using Domain.Users.Entities;
using MediatR;

namespace Application.Users.Queries;

public record GetUserByIdQuery(int UserId) : IQuery<User?>;

public class GetUserByIdQueryHandler(IUserRepository userRepository)
	: IRequestHandler<GetUserByIdQuery, User?>
{
	private readonly IUserRepository _userRepository = userRepository;

	public async Task<User?> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
	{
		return await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
	}
}
