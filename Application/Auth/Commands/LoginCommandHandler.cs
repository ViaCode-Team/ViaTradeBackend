using Application.Auth.Interfaces;
using Application.Common.Models;
using Application.Users.Interfaces;
using Application.Users.Models;
using MediatR;

namespace Application.Auth.Commands;

public class LoginCommandHandler(
	IUserRepository userRepository,
	IPasswordHasher passwordHasher,
	IJwtHelper jwtHelper,
	ISessionRepository sessionRepository,
	IRefreshTokenRepository refreshTokenRepository)
	: IRequestHandler<LoginCommand, AuthInternalResult>
{
	private readonly IUserRepository _userRepository = userRepository;
	private readonly IPasswordHasher _passwordHasher = passwordHasher;
	private readonly IJwtHelper _jwtHelper = jwtHelper;
	private readonly ISessionRepository _sessionRepository = sessionRepository;
	private readonly IRefreshTokenRepository _refreshTokenRepository = refreshTokenRepository;
	private readonly TimeSpan _sessionTtl = TimeSpan.FromDays(7);

	public async Task<AuthInternalResult> Handle(LoginCommand request, CancellationToken cancellationToken)
	{
		var user = await _userRepository.GetByLoginAsync(request.Login, cancellationToken);

		if (user == null || !_passwordHasher.Verify(request.Password, user.HashPassword))
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
}
