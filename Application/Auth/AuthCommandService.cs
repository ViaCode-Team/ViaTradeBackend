using Application.Auth.Interfaces;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Users.Interfaces;
using Application.Users.Models;
using Domain.Users.Entities;

namespace Application.Auth;

public class AuthCommandService(
	IUserRepository userRepository,
	IPasswordHasher passwordHasher,
	IJwtHelper jwtHelper,
	ISessionRepository sessionRepository,
	IRefreshTokenRepository refreshTokenRepository,
	IUnitOfWork uow) : IAuthCommandService
{
	private readonly TimeSpan _sessionTtl = TimeSpan.FromDays(7);

	public async Task<AuthInternalResult> LoginAsync(string login, string password, string userAgent, CancellationToken ct)
	{
		var user = await userRepository.GetByLoginAsync(login, ct);

		if (user == null || !passwordHasher.Verify(password, user.HashPassword))
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

		await sessionRepository.CreateAsync(session, _sessionTtl);

		var accessToken = jwtHelper.GenerateAccessToken(user, sessionId);
		var refreshToken = jwtHelper.GenerateRefreshToken();

		await refreshTokenRepository.StoreAsync(sessionId, refreshToken, _sessionTtl);

		return new AuthInternalResult
		{
			AccessToken = accessToken,
			RefreshToken = refreshToken
		};
	}

	public async Task LogoutAllAsync(int userId, CancellationToken ct)
	{
		var sessions = await sessionRepository.GetUserSessionsAsync(userId);

		foreach (var session in sessions)
		{
			await refreshTokenRepository.RemoveAsync(session.Id);
			await sessionRepository.RemoveAsync(session.Id);
		}
	}

	public async Task LogoutSessionAsync(string refreshToken, CancellationToken ct)
	{
		var sessionId = await refreshTokenRepository.GetSessionIdAsync(refreshToken);
		if (sessionId == null)
			return;

		await refreshTokenRepository.RemoveAsync(sessionId);
		await sessionRepository.RemoveAsync(sessionId);
	}

	public async Task<AuthInternalResult> RefreshTokenAsync(string refreshToken, CancellationToken ct)
	{
		var sessionId = await refreshTokenRepository.GetSessionIdAsync(refreshToken)
			?? throw new UnauthorizedAccessException();

		var session = await sessionRepository.GetAsync(sessionId)
			?? throw new UnauthorizedAccessException();

		var user = await userRepository.GetByIdAsync(session.UserId, ct)
			?? throw new UnauthorizedAccessException();

		session.LastSeen = DateTime.UtcNow;
		await sessionRepository.CreateAsync(session, _sessionTtl);

		var newAccessToken = jwtHelper.GenerateAccessToken(user, sessionId);
		var newRefreshToken = jwtHelper.GenerateRefreshToken();

		await refreshTokenRepository.RotateAsync(sessionId, newRefreshToken, _sessionTtl);

		return new AuthInternalResult
		{
			AccessToken = newAccessToken,
			RefreshToken = newRefreshToken
		};
	}

	public async Task<AuthInternalResult> RegisterAsync(string login, string password, string userAgent, CancellationToken ct)
	{
		if (await userRepository.ExistsAsync(u => u.Login == login, ct))
			throw new InvalidOperationException("User already exists");

		var user = new User
		{
			Login = login,
			HashPassword = passwordHasher.Hash(password),
			RegisterDate = DateTime.UtcNow
		};

		await userRepository.AddAsync(user, ct);
		await uow.SaveChangesAsync(ct);

		return await LoginAsync(login, password, userAgent, ct);
	}
}
