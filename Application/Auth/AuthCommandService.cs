using Application.Auth.Interfaces;
using Application.Auth.Models;
using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Users.Interfaces;
using Application.Users.Models;
using Domain.Entities;

namespace Application.Auth;

public class AuthCommandService(
	IUserRepository userRepository,
	IPasswordHasher passwordHasher,
	IJwtHelper jwtHelper,
	ISessionRepository sessionRepository,
	IRefreshTokenRepository refreshTokenRepository,
	IUnitOfWork uow,
	SessionLifetimeOptions sessionLifetimeOptions
) : IAuthCommandService
{
	private readonly TimeSpan _idleSessionLifetime = sessionLifetimeOptions.IdleLifetime;
	private readonly TimeSpan _absoluteSessionLifetime = sessionLifetimeOptions.AbsoluteLifetime;

	public async Task<AuthTokens> LoginAsync(string login, string password, string userAgent, CancellationToken ct)
	{
		var user = await userRepository.FindLoginUserAsync(login, ct);

		if (user == null || !passwordHasher.Verify(password, user.PasswordHash))
			throw new InvalidCredentialsException();

		var now = DateTime.UtcNow;
		var sessionId = Guid.NewGuid().ToString();
		var session = new UserSessionDto
		{
			Id = sessionId,
			UserId = user.Id,
			UserAgent = userAgent,
			CreatedAt = now,
			LastSeen = now,
			ExpiresAt = CalculateExpiresAt(now, now),
		};
		var sessionTtl = session.ExpiresAt - now;

		await sessionRepository.CreateAsync(session, sessionTtl);

		var accessToken = jwtHelper.GenerateAccessToken(new UserTokenDto(user.Id, user.Login), sessionId);
		var refreshToken = jwtHelper.GenerateRefreshToken();

		await refreshTokenRepository.StoreAsync(sessionId, refreshToken, sessionTtl);

		return new AuthTokens { AccessToken = accessToken, RefreshToken = refreshToken };
	}

	public async Task LogoutAllAsync(int userId, CancellationToken ct)
	{
		var sessions = await sessionRepository.ListByUserAsync(userId);

		foreach (var session in sessions)
		{
			await refreshTokenRepository.RemoveAsync(session.Id);
			await sessionRepository.RemoveAsync(session.Id);
		}
	}

	public async Task LogoutSessionAsync(string refreshToken, CancellationToken ct)
	{
		var sessionId = await refreshTokenRepository.FindSessionIdAsync(refreshToken);
		if (sessionId == null)
			return;

		await refreshTokenRepository.RemoveAsync(sessionId);
		await sessionRepository.RemoveAsync(sessionId);
	}

	public async Task<AuthTokens> RefreshTokenAsync(string refreshToken, CancellationToken ct)
	{
		var sessionId = await refreshTokenRepository.FindSessionIdAsync(refreshToken);
		if (sessionId == null)
			throw new InvalidTokenException();

		var session = await sessionRepository.FindByIdAsync(sessionId);
		if (session == null)
			throw new InvalidTokenException();

		var user = await userRepository.FindTokenUserAsync(session.UserId, ct);
		if (user == null)
			throw new InvalidTokenException();

		var now = DateTime.UtcNow;
		session.LastSeen = now;
		session.ExpiresAt = CalculateExpiresAt(session.CreatedAt, now);
		if (session.ExpiresAt <= now)
		{
			await refreshTokenRepository.RemoveAsync(sessionId);
			await sessionRepository.RemoveAsync(sessionId);
			throw new InvalidTokenException();
		}

		var sessionTtl = session.ExpiresAt - now;
		await sessionRepository.CreateAsync(session, sessionTtl);

		var newAccessToken = jwtHelper.GenerateAccessToken(user, sessionId);
		var newRefreshToken = jwtHelper.GenerateRefreshToken();

		await refreshTokenRepository.RotateAsync(sessionId, newRefreshToken, sessionTtl);

		return new AuthTokens { AccessToken = newAccessToken, RefreshToken = newRefreshToken };
	}

	public async Task<AuthTokens> RegisterAsync(string login, string password, string userAgent, CancellationToken ct)
	{
		if (await userRepository.ExistsAsync(u => u.Login == login, ct))
			throw new ConflictException("User already exists.", "user_already_exists");

		var user = new User
		{
			Login = login,
			PasswordHash = passwordHasher.Hash(password),
			RegisteredAt = DateTime.UtcNow,
		};

		await userRepository.AddAsync(user, ct);
		await uow.SaveChangesAsync(ct);

		return await LoginAsync(login, password, userAgent, ct);
	}

	private DateTime CalculateExpiresAt(DateTime createdAt, DateTime now)
	{
		var idleExpiresAt = now.Add(_idleSessionLifetime);
		var absoluteExpiresAt = createdAt.Add(_absoluteSessionLifetime);
		if (idleExpiresAt < absoluteExpiresAt)
			return idleExpiresAt;

		return absoluteExpiresAt;
	}
}
