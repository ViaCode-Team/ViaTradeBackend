using Application.Common.Interfaces;
using Application.Users.Interfaces;
using MediatR;

namespace Application.Users.Commands;

public record LinkTelegramCommand(string TgToken, string TgId) : ICommandWithoutUoW;

public class LinkTelegramCommandHandler(
	IUserRepository userRepository,
	ITgTokenRepository tgTokenRepository)
	: IRequestHandler<LinkTelegramCommand>
{
	private readonly IUserRepository _userRepository = userRepository;
	private readonly ITgTokenRepository _tgTokenRepository = tgTokenRepository;

	public async Task Handle(LinkTelegramCommand request, CancellationToken cancellationToken)
	{
		var userIdNullable = await _tgTokenRepository.GetUserIdAsync(request.TgToken);
		if (userIdNullable == null)
			throw new NullReferenceException(nameof(request.TgToken));

		await _tgTokenRepository.RemoveAsync(request.TgToken);
		var userId = userIdNullable.Value;

		var affectedRows = await _userRepository.UpdateTgIdAsync(userId, request.TgId, cancellationToken);
		if (affectedRows == 0)
			throw new NullReferenceException(nameof(request.TgToken));
	}
}
