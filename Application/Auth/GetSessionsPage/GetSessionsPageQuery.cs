using ViaTrade.Application.Common.Abstractions;
using ViaTrade.Application.Common.Models;

namespace ViaTrade.Application.Auth.GetSessionsPage;

public sealed record GetSessionsPageQuery(int UserId, PageOptions PageOptions, string? CurrentSessionId = null)
	: IQuery<PageResult<SessionResult>>;
