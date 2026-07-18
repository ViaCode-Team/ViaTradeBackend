using Application.Contracts.Dto.User;
using Application.Interfaces.Repositories.Database;
using Application.Interfaces.Repositories.Redis;
using Application.Interfaces.Services;
using Application.Interfaces.Utils;
using Application.Models;
using Domain.Entities.DataBase;
using Domain.Models.Pagination;

namespace Application.Services;

public class AuthService(
	IUserRepository userRepository,
	IPasswordHasher passwordHasher,
	IJwtHelper jwtHelper,
	ISessionRepository sessionRepository,
	IRefreshTokenRepository refreshTokenRepository) : IAuthService
{
	private readonly IUserRepository _userRepository = userRepository;
	private readonly IPasswordHasher _passwordHasher = passwordHasher;
	private readonly IJwtHelper _jwtHelper = jwtHelper;
	private readonly ISessionRepository _sessionRepository = sessionRepository;
	private readonly IRefreshTokenRepository _refreshTokenRepository = refreshTokenRepository;

	private readonly TimeSpan _sessionTtl = TimeSpan.FromDays(7);

	public async Task<AuthInternalResult> LoginAsync(
		string login,
		string password,
		string userAgent,
		CancellationToken cancellationToken = default)
	{
		var user = await _userRepository.GetByLoginAsync(login, cancellationToken);

		if (user == null || !_passwordHasher.Verify(password, user.HashPassword))
			throw new UnauthorizedAccessException();

		var sessionId = Guid.NewGuid().ToString();

		var session = new UserSessionDto
		{
			Id = sessionId,
			UserId = user.Id,
			UserAgent = userAgent,
			CreatedAt = DateTime.UtcNow,
			LastSeen = DateTime.UtcNow
		};

		await _sessionRepository.CreateAsync(session, _sessionTtl);

		var accessToken = _jwtHelper.GenerateAccessToken(user, sessionId);
		var refreshToken = _jwtHelper.GenerateRefreshToken();

		await _refreshTokenRepository.StoreAsync(sessionId, refreshToken, _sessionTtl);

		return new AuthInternalResult
		{
			AccessToken = accessToken,
			RefreshToken = refreshToken
		};
	}

	public async Task<AuthInternalResult> RefreshTokenAsync(
		string refreshToken,
		CancellationToken cancellationToken = default)
	{
		var sessionId = await _refreshTokenRepository.GetSessionIdAsync(refreshToken)
			?? throw new UnauthorizedAccessException();

		var session = await _sessionRepository.GetAsync(sessionId)
			?? throw new UnauthorizedAccessException();

		var user = await _userRepository.GetByIdAsync(session.UserId, cancellationToken)
			?? throw new UnauthorizedAccessException();

		session.LastSeen = DateTime.UtcNow;
		await _sessionRepository.CreateAsync(session, _sessionTtl);

		var newAccessToken = _jwtHelper.GenerateAccessToken(user, sessionId);
		var newRefreshToken = _jwtHelper.GenerateRefreshToken();

		await _refreshTokenRepository.RotateAsync(sessionId, newRefreshToken, _sessionTtl);

		return new AuthInternalResult
		{
			AccessToken = newAccessToken,
			RefreshToken = newRefreshToken
		};
	}

	public async Task LogoutSessionAsync(string refreshToken)
	{
		var sessionId = await _refreshTokenRepository.GetSessionIdAsync(refreshToken);
		if (sessionId == null)
			return;

		await _refreshTokenRepository.RemoveAsync(sessionId);
		await _sessionRepository.RemoveAsync(sessionId);
	}

	public async Task LogoutAllAsync(int userId)
	{
		var sessions = await _sessionRepository.GetUserSessionsAsync(userId);

		foreach (var session in sessions)
		{
			await _refreshTokenRepository.RemoveAsync(session.Id);
			await _sessionRepository.RemoveAsync(session.Id);
		}
	}

	public async Task<AuthInternalResult> RegisterAsync(
		string login,
		string password,
		CancellationToken cancellationToken = default)
	{
		if (await _userRepository.ExistsAsync(u => u.Login == login, cancellationToken))
			throw new InvalidOperationException();

		var user = new User
		{
			Login = login,
			HashPassword = _passwordHasher.Hash(password),
			LastLoginDate = DateTime.UtcNow,
			RegisterDate = DateTime.UtcNow,
		};

		await _userRepository.AddAsync(user, cancellationToken);
		await _userRepository.SaveChangesAsync(cancellationToken);

		return await LoginAsync(login, password, "initial", cancellationToken);
	}

	public async Task<IEnumerable<UserSessionDto>> GetUserSessionsAsync(int userId)
	{
		return await _sessionRepository.GetUserSessionsAsync(userId);
	}

	public async Task<PagedResult<UserSessionDto>> GetPagedUserSessionsAsync(int userId, PaginationRequest paginationRequest)
	{
		return await _sessionRepository.GetPagedUserSessionsAsync(userId, paginationRequest);
	}
}
