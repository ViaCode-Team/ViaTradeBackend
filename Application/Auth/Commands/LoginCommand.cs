using Application.Auth.Interfaces;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Users.Interfaces;
using Application.Users.Models;
using MediatR;

namespace Application.Auth.Commands;

public record LoginCommand(string Login, string Password, string UserAgent) : ICommand<AuthInternalResult>;

public class LoginCommandHandler(
	IUserRepository userRepository,
	IPasswordHasher passwordHasher,
	IJwtHelper jwtHelper,
	ISessionRepository sessionRepository,
	IRefreshTokenRepository refreshTokenRepository)
	: IRequestHandler<LoginCommand, AuthInternalResult>
{
	private readonly TimeSpan _sessionTtl = TimeSpan.FromDays(7);

	public async Task<AuthInternalResult> Handle(LoginCommand request, CancellationToken ct)
	{
		var user = await userRepository.GetByLoginAsync(request.Login, ct);

		if (user == null || !passwordHasher.Verify(request.Password, user.HashPassword))
			throw new UnauthorizedAccessException();

		var sessionId = Guid.NewGuid().ToString();

		var session = new UserSessionDto
		{
			Id = sessionId,
			UserId = user.Id,
			UserAgent = request.UserAgent,
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
}
