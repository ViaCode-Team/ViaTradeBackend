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
	IUnitOfWork uow,
	SessionLifetimeOptions sessionLifetimeOptions
) : IAuthCommandService
{
	private readonly TimeSpan _accessTokenLifetime = sessionLifetimeOptions.AccessTokenLifetime;
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
		var refreshToken = jwtHelper.GenerateRefreshToken();

		await sessionRepository.CreateSessionAsync(session, refreshToken, sessionTtl);

		return CreateAuthTokens(new UserTokenDto(user.Id, user.Login), session, refreshToken, now);
	}

	public async Task LogoutAllAsync(int userId, CancellationToken ct)
	{
		var sessions = await sessionRepository.ListByUserAsync(userId);

		foreach (var session in sessions)
			await sessionRepository.TerminateSessionAsync(session.Id);
	}

	public async Task LogoutSessionAsync(string sessionId, CancellationToken ct)
	{
		await sessionRepository.TerminateSessionAsync(sessionId);
	}

	public async Task<AuthTokens> RefreshTokenAsync(string refreshToken, CancellationToken ct)
	{
		var session = await sessionRepository.FindByRefreshTokenAsync(refreshToken);
		if (session == null)
		{
			await sessionRepository.TryTerminateSessionByUsedRefreshTokenAsync(refreshToken);
			throw new InvalidTokenException();
		}

		var user = await userRepository.FindTokenUserAsync(session.UserId, ct);
		if (user == null)
		{
			await sessionRepository.TerminateSessionAsync(session.Id);
			throw new InvalidTokenException();
		}

		var now = DateTime.UtcNow;
		session.LastSeen = now;
		session.ExpiresAt = CalculateExpiresAt(session.CreatedAt, now);
		if (session.ExpiresAt <= now)
		{
			await sessionRepository.TerminateSessionAsync(session.Id);
			throw new InvalidTokenException();
		}

		var sessionTtl = session.ExpiresAt - now;
		var usedRefreshTokenTtl = session.CreatedAt.Add(_absoluteSessionLifetime) - now;
		var newRefreshToken = jwtHelper.GenerateRefreshToken();

		if (
			!await sessionRepository.TryRotateRefreshAsync(
				session,
				refreshToken,
				newRefreshToken,
				sessionTtl,
				usedRefreshTokenTtl
			)
		)
		{
			await sessionRepository.TryTerminateSessionByUsedRefreshTokenAsync(refreshToken);
			throw new InvalidTokenException();
		}

		return CreateAuthTokens(user, session, newRefreshToken, now);
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

	private AuthTokens CreateAuthTokens(
		UserTokenDto user,
		UserSessionDto session,
		string refreshToken,
		DateTime now
	)
	{
		var configuredAccessTokenExpiresAt = now.Add(_accessTokenLifetime);
		var accessTokenExpiresAt = configuredAccessTokenExpiresAt;
		if (session.ExpiresAt < configuredAccessTokenExpiresAt)
			accessTokenExpiresAt = session.ExpiresAt;

		var accessToken = jwtHelper.GenerateAccessToken(user, session.Id, accessTokenExpiresAt);

		return new AuthTokens
		{
			AccessToken = accessToken,
			RefreshToken = refreshToken,
			AccessTokenExpiresAt = new DateTimeOffset(accessTokenExpiresAt),
			RefreshTokenExpiresAt = new DateTimeOffset(session.ExpiresAt),
		};
	}
}
