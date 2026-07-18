using Application.Auth.Interfaces;
using Application.Common.Interfaces;
using Application.Users.Models;
using MediatR;

namespace Application.Auth.Queries;

public record GetUserSessionsQuery(int UserId) : IQuery<IEnumerable<UserSessionDto>>;

public class GetUserSessionsQueryHandler(ISessionRepository sessionRepository)
	: IRequestHandler<GetUserSessionsQuery, IEnumerable<UserSessionDto>>
{
	private readonly ISessionRepository _sessionRepository = sessionRepository;

	public async Task<IEnumerable<UserSessionDto>> Handle(GetUserSessionsQuery request, CancellationToken cancellationToken)
	{
		return await _sessionRepository.GetUserSessionsAsync(request.UserId);
	}
}
