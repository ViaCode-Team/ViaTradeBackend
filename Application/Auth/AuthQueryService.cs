using Application.Auth.Interfaces;
using Application.Common.Models;
using Application.Users.Models;

namespace Application.Auth;

public class AuthQueryService(ISessionRepository sessionRepository) : IAuthQueryService
{
	public async Task<PageResult<UserSessionDto>> GetSessionsPageAsync(
		int userId,
		PageOptions page,
		CancellationToken ct
	)
	{
		return await sessionRepository.GetPageByUserAsync(userId, page);
	}
}
