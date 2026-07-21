using Application.Auth.Interfaces;
using Application.Auth.Models;
using Application.Common.Exceptions;
using Application.Common.Interfaces;
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
	IUnitOfWork uow
) : IAuthCommandService
{
	private readonly TimeSpan _sessionTtl = TimeSpan.FromDays(7);

	public async Task<AuthTokens> LoginAsync(string login, string password, string userAgent, CancellationToken ct)
	{
		var user = await userRepository.GetLoginUserAsync(login, ct);

		if (user == null || !passwordHasher.Verify(password, user.HashPassword))
			throw new InvalidCredentialsException();

		var sessionId = Guid.NewGuid().ToString();
		var session = new UserSessionDto
		{
			Id = sessionId,
			UserId = user.Id,
			UserAgent = userAgent,
			CreatedAt = DateTime.UtcNow,
			LastSeen = DateTime.UtcNow,
		};

		await sessionRepository.CreateAsync(session, _sessionTtl);

		var accessToken = jwtHelper.GenerateAccessToken(new UserTokenDto(user.Id, user.Login), sessionId);
		var refreshToken = jwtHelper.GenerateRefreshToken();

		await refreshTokenRepository.StoreAsync(sessionId, refreshToken, _sessionTtl);

		return new AuthTokens { AccessToken = accessToken, RefreshToken = refreshToken };
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

	public async Task<AuthTokens> RefreshTokenAsync(string refreshToken, CancellationToken ct)
	{
		var sessionId = await refreshTokenRepository.GetSessionIdAsync(refreshToken);
		if (sessionId == null)
			throw new InvalidTokenException();

		var session = await sessionRepository.GetAsync(sessionId);
		if (session == null)
			throw new InvalidTokenException();

		var user = await userRepository.GetTokenUserAsync(session.UserId, ct);
		if (user == null)
			throw new InvalidTokenException();

		session.LastSeen = DateTime.UtcNow;
		await sessionRepository.CreateAsync(session, _sessionTtl);

		var newAccessToken = jwtHelper.GenerateAccessToken(user, sessionId);
		var newRefreshToken = jwtHelper.GenerateRefreshToken();

		await refreshTokenRepository.RotateAsync(sessionId, newRefreshToken, _sessionTtl);

		return new AuthTokens { AccessToken = newAccessToken, RefreshToken = newRefreshToken };
	}

	public async Task<AuthTokens> RegisterAsync(string login, string password, string userAgent, CancellationToken ct)
	{
		if (await userRepository.ExistsAsync(u => u.Login == login, ct))
			throw new ConflictException("User already exists.", "user_already_exists");

		var user = new User
		{
			Login = login,
			HashPassword = passwordHasher.Hash(password),
			RegisterDate = DateTime.UtcNow,
		};

		await userRepository.AddAsync(user, ct);
		await uow.SaveChangesAsync(ct);

		return await LoginAsync(login, password, userAgent, ct);
	}
}
