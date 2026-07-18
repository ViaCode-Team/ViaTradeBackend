using Application.Contracts.Dto.User;
using Application.Models;
using Domain.Models.Pagination;
namespace Application.Interfaces.Services;

public interface IAuthService
{
	Task<AuthInternalResult> LoginAsync(string login, string password, string userAgent, CancellationToken cancellationToken);
	Task<AuthInternalResult> RegisterAsync(string login, string password, CancellationToken cancellationToken);
	Task<AuthInternalResult> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken);
	Task LogoutSessionAsync(string refreshToken);
	Task LogoutAllAsync(int userId);
	Task<IEnumerable<UserSessionDto>> GetUserSessionsAsync(int userId);
	Task<PagedResult<UserSessionDto>> GetPagedUserSessionsAsync(int userId, PaginationRequest paginationRequest);
}
