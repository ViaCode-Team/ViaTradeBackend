using Application.Common.Models;
using Application.Users.Models;

namespace Application.Auth.Interfaces;

public interface IAuthQueryService
{
	Task<PageResult<UserSessionDto>> GetSessionsPagedAsync(int userId, PageOptions page, CancellationToken ct);
	Task<IEnumerable<UserSessionDto>> GetSessionsAsync(int userId, CancellationToken ct);
}
