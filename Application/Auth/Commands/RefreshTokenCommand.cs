using Application.Auth.Interfaces;
using Application.Common.Models;
using Application.Users.Interfaces;
using MediatR;

namespace Application.Auth.Commands;

public record RefreshTokenCommand(string RefreshToken) : IRequest<AuthInternalResult>;

public class RefreshTokenCommandHandler(
	IUserRepository userRepository,
	IJwtHelper jwtHelper,
	ISessionRepository sessionRepository,
	IRefreshTokenRepository refreshTokenRepository)
	: IRequestHandler<RefreshTokenCommand, AuthInternalResult>
{
	private readonly IUserRepository _userRepository = userRepository;
	private readonly IJwtHelper _jwtHelper = jwtHelper;
	private readonly ISessionRepository _sessionRepository = sessionRepository;
	private readonly IRefreshTokenRepository _refreshTokenRepository = refreshTokenRepository;

	private readonly TimeSpan _sessionTtl = TimeSpan.FromDays(7);

	public async Task<AuthInternalResult> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
	{
		var sessionId = await _refreshTokenRepository.GetSessionIdAsync(request.RefreshToken)
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
}
