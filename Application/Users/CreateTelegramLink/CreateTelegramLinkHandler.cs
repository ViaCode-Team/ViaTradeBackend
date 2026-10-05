using System.Security.Cryptography;
using Microsoft.Extensions.Options;
using ViaTrade.Application.Common.Abstractions;
using ViaTrade.Application.Common.Abstractions.Repositories;
using ViaTrade.Application.Users.Common;
using ViaTrade.Configuration.Options;

namespace ViaTrade.Application.Users.CreateTelegramLink;

public sealed class CreateTelegramLinkHandler(
	ICacheRepository<TelegramTokenEntity> telegramTokenRepository,
	IOptions<TelegramBotSettings> telegramBotOptions
) : ICommandHandler<CreateTelegramLinkCommand, TelegramLinkResult>
{
	public async Task<TelegramLinkResult> HandleAsync(CreateTelegramLinkCommand command, CancellationToken ct = default)
	{
		var token = Convert
			.ToBase64String(RandomNumberGenerator.GetBytes(24))
			.Replace("+", "-")
			.Replace("/", "_")
			.TrimEnd('=');

		await telegramTokenRepository.SetAsync(
			new TelegramTokenEntity { Id = token, UserId = command.UserId },
			TimeSpan.FromMinutes(5)
		);

		return new TelegramLinkResult($"https://t.me/{telegramBotOptions.Value.BotUsername}?start={token}");
	}
}
