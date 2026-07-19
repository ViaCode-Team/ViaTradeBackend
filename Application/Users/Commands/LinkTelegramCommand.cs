using Application.Common.Interfaces;
using Application.Users.Interfaces;
using MediatR;

namespace Application.Users.Commands;

public record LinkTelegramCommand(string TgToken, string TgId) : ICommand;

public class LinkTelegramCommandHandler(
	IUserRepository userRepository, ITgTokenRepository tgTokenRepository)
	: IRequestHandler<LinkTelegramCommand>
{
	public async Task Handle(LinkTelegramCommand request, CancellationToken cancellationToken)
	{
		var userIdNullable = await tgTokenRepository.GetUserIdAsync(request.TgToken);
		if (userIdNullable == null)
			throw new NullReferenceException(nameof(request.TgToken));

		await tgTokenRepository.RemoveAsync(request.TgToken);
		var userId = userIdNullable.Value;

		var affectedRows = await userRepository.UpdateTgIdAsync(userId, request.TgId, cancellationToken);
		if (affectedRows == 0)
			throw new NullReferenceException(nameof(request.TgToken));
	}
}
