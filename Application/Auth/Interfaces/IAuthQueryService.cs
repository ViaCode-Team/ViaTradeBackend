using ViaTrade.Application.Common.Models;
using ViaTrade.Application.Users.Models;

namespace ViaTrade.Application.Auth.Interfaces;

public interface IAuthQueryService
{
	Task<PageResult<UserSessionDto>> GetSessionsPageAsync(int userId, PageOptions pageOptions, CancellationToken ct);
}
