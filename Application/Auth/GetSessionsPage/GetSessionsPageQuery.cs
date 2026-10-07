using ViaTrade.Application.Common.Abstractions;
using ViaTrade.Application.Common.Models;

namespace ViaTrade.Application.Auth.GetSessionsPage;

public sealed record GetSessionsPageQuery(PageOptions PageOptions) : IQuery<PageResult<SessionResult>>;
