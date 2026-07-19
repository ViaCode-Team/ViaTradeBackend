using Application.Common.Interfaces;
using Application.Users.Interfaces;
using Domain.Users.Entities;
using MediatR;

namespace Application.Users.Queries;

public record GetUserByLoginQuery(string Login) : IQuery<User?>;

public class GetUserByLoginQueryHandler(IUserRepository userRepository)
	: IRequestHandler<GetUserByLoginQuery, User?>
{
	public async Task<User?> Handle(GetUserByLoginQuery request, CancellationToken ct)
	{
		return await userRepository.GetByLoginAsync(request.Login, ct);
	}
}
