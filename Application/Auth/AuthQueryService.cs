using Application.Auth.Interfaces;
using Application.Common.Queries;
using Application.Users.Models;

namespace Application.Auth;

public class AuthQueryService(ISessionRepository sessionRepository) : IAuthQueryService
{
	public async Task<PageResult<UserSessionDto>> GetSessionsPagedAsync(int userId, PageOptions page, CancellationToken ct)
	{
		return await sessionRepository.GetPagedUserSessionsAsync(userId, page);
	}

	public async Task<IEnumerable<UserSessionDto>> GetSessionsAsync(int userId, CancellationToken ct)
	{
		return await sessionRepository.GetUserSessionsAsync(userId);
	}
}
