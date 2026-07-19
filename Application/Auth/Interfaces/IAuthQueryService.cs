using Application.Common.Models.Pagination;
using Application.Users.Models;

namespace Application.Auth.Interfaces;

public interface IAuthQueryService
{
	Task<PagedResult<UserSessionDto>> GetSessionsPagedAsync(int userId, PaginationRequest paginationRequest, CancellationToken ct);
	Task<IEnumerable<UserSessionDto>> GetSessionsAsync(int userId, CancellationToken ct);
}
