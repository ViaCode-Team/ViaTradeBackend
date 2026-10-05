using ViaTrade.Application.Common.Abstractions;
using ViaTrade.Application.Common.Abstractions.Repositories;
using ViaTrade.Application.Users.Common;

namespace ViaTrade.Application.Users.ConsumeTelegramToken;

public sealed class ConsumeTelegramTokenHandler(ICacheRepository<TelegramTokenEntity> telegramTokenRepository)
	: ICommandHandler<ConsumeTelegramTokenCommand, TelegramTokenResult?>
{
	public async Task<TelegramTokenResult?> HandleAsync(
		ConsumeTelegramTokenCommand command,
		CancellationToken ct = default
	)
	{
		var token = await telegramTokenRepository.ConsumeAsync(command.TelegramToken);
		if (token == null)
			return null;

		return new TelegramTokenResult(token.UserId);
	}
}
