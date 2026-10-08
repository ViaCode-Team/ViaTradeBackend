using System.Security.Cryptography;
using Mediator;
using Microsoft.Extensions.Options;
using ViaTrade.Application.Common.Abstractions;
using ViaTrade.Application.Common.Abstractions.Repositories;
using ViaTrade.Application.Users.Common;
using ViaTrade.Configuration.Options;

namespace ViaTrade.Application.Users.CreateTelegramLink;

public sealed class CreateTelegramLinkHandler(
	IUserContext userContext,
	ICacheRepository<TelegramTokenEntity> telegramTokenRepository,
	IOptions<TelegramBotSettings> telegramBotOptions
) : ICommandHandler<CreateTelegramLinkCommand, TelegramLinkResult>
{
	public async ValueTask<TelegramLinkResult> Handle(CreateTelegramLinkCommand command, CancellationToken ct)
	{
		var token = Convert
			.ToBase64String(RandomNumberGenerator.GetBytes(24))
			.Replace("+", "-")
			.Replace("/", "_")
			.TrimEnd('=');

		await telegramTokenRepository.SetAsync(
			new TelegramTokenEntity { Id = token, UserId = userContext.UserId },
			TimeSpan.FromMinutes(5)
		);

		return new TelegramLinkResult($"https://t.me/{telegramBotOptions.Value.BotUsername}?start={token}");
	}
}
