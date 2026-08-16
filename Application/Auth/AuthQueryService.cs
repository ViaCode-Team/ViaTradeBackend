using ViaTrade.Application.Auth.Interfaces;
using ViaTrade.Application.Common.Models;
using ViaTrade.Application.Users.Models;

namespace ViaTrade.Application.Auth;

public class AuthQueryService(ISessionRepository sessionRepository) : IAuthQueryService
{
	public async Task<PageResult<UserSessionDto>> GetSessionsPageAsync(
		int userId,
		PageOptions pageOptions,
		CancellationToken ct
	)
	{
		return await sessionRepository.GetPageByUserAsync(userId, pageOptions);
	}
}
