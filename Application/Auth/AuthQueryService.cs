using ViaTrade.Application.Auth.Interfaces;
using ViaTrade.Application.Common.Models;
using ViaTrade.Application.Common.Queries;
using ViaTrade.Application.Common.Specifications;
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
		var specification = new PageSpecification<UserSessionDto>(pageOptions);
		ct.ThrowIfCancellationRequested();
		var sessions = await sessionRepository.ListByUserAsync(userId);
		return PageQuery.FromList(sessions, specification);
	}
}
