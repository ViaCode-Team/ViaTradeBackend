using System.Security.Cryptography;
using Microsoft.Extensions.Options;
using ViaTrade.Application.Common.Exceptions;
using ViaTrade.Application.Common.Interfaces.Repositories;
using ViaTrade.Application.Users.Interfaces;
using ViaTrade.Application.Users.Models;
using ViaTrade.Configuration.Options;

namespace ViaTrade.Application.Users;

public class UserCommandService(
	IUserRepository userRepository,
	ICacheRepository<TelegramTokenEntity> telegramTokenRepository,
	IOptions<TelegramBotSettings> telegramBotOptions
) : IUserCommandService
{
	public async Task<string> GenerateTgLinkAsync(int userId, CancellationToken ct)
	{
		var token = Convert
			.ToBase64String(RandomNumberGenerator.GetBytes(24))
			.Replace("+", "-")
			.Replace("/", "_")
			.TrimEnd('=');

		await telegramTokenRepository.SetAsync(
			new TelegramTokenEntity { Id = token, UserId = userId },
			TimeSpan.FromMinutes(5)
		);

		return $"https://t.me/{telegramBotOptions.Value.BotUsername}?start={token}";
	}

	public async Task LinkTelegramAsync(string telegramToken, string telegramId, CancellationToken ct)
	{
		var token = await telegramTokenRepository.ConsumeAsync(telegramToken);
		if (token == null)
			throw new InvalidTokenException("The Telegram link token is invalid or expired.");

		var userId = token.UserId;

		var affectedRows = await userRepository.ExecuteUpdateTelegramIdAsync(userId, telegramId, ct);
		if (affectedRows == 0)
			throw new NotFoundException("User not found.", "user_not_found");
	}

	public async Task UpdateLastLoginAtAsync(int userId, CancellationToken ct)
	{
		await userRepository.ExecuteUpdateLastLoginAtAsync(userId, DateTime.UtcNow, ct);
	}
}
