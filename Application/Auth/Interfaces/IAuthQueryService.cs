using Application.Common.Models;
using Application.Users.Models;

namespace Application.Auth.Interfaces;

public interface IAuthQueryService
{
	Task<PageResult<UserSessionDto>> GetSessionsPageAsync(int userId, PageOptions page, CancellationToken ct);
}
