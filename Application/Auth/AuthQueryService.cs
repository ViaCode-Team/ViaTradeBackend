using Application.Auth.Interfaces;
using Application.Common.Models.Pagination;
using Application.Users.Models;

namespace Application.Auth;

public class AuthQueryService(ISessionRepository sessionRepository) : IAuthQueryService
{
	public async Task<PagedResult<UserSessionDto>> GetSessionsPagedAsync(int userId, PaginationRequest paginationRequest, CancellationToken ct)
	{
		return await sessionRepository.GetPagedUserSessionsAsync(userId, paginationRequest);
	}

	public async Task<IEnumerable<UserSessionDto>> GetSessionsAsync(int userId, CancellationToken ct)
	{
		return await sessionRepository.GetUserSessionsAsync(userId);
	}
}
