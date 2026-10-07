using ViaTrade.Application.Common.Abstractions;
using ViaTrade.Application.Common.Abstractions.Repositories;
using ViaTrade.Application.Common.Exceptions;
using ViaTrade.Application.Users.Common;
using ViaTrade.Application.Users.Common.Abstractions;

namespace ViaTrade.Application.Users.LinkTelegram;

public sealed class LinkTelegramHandler(
	IUserRepository userRepository,
	ICacheRepository<TelegramTokenEntity> telegramTokenRepository
) : ICommandHandler<LinkTelegramCommand>
{
	public async Task HandleAsync(LinkTelegramCommand command, CancellationToken ct)
	{
		var token = await telegramTokenRepository.ConsumeAsync(command.TelegramToken);
		if (token == null)
			throw new InvalidTokenException("The Telegram link token is invalid or expired.");

		var userId = token.UserId;

		var affectedRows = await userRepository.ExecuteUpdateTelegramIdAsync(userId, command.TelegramId, ct);
		if (affectedRows == 0)
			throw new NotFoundException("User not found.", "user_not_found");
	}
}
