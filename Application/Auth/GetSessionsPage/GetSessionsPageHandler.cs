using ViaTrade.Application.Auth.Common;
using ViaTrade.Application.Auth.Common.Abstractions;
using ViaTrade.Application.Common.Abstractions;
using ViaTrade.Application.Common.Models;

namespace ViaTrade.Application.Auth.GetSessionsPage;

public sealed class GetSessionsPageHandler(IUserContext userContext, ISessionRepository sessionRepository)
	: IQueryHandler<GetSessionsPageQuery, PageResult<SessionResult>>
{
	public async Task<PageResult<SessionResult>> HandleAsync(GetSessionsPageQuery query, CancellationToken ct)
	{
		ct.ThrowIfCancellationRequested();

		var sessions = await sessionRepository.ListByUserAsync(userContext.UserId);
		return PageResult<SessionData>
			.FromList(sessions, query.PageOptions.Page, query.PageOptions.PageSize)
			.Map(session => SessionResult.Projection(session) with { IsCurrent = session.Id == userContext.SessionId });
	}
}
