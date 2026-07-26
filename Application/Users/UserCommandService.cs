using System.Security.Cryptography;
using Application.Common.Exceptions;
using Application.Users.Interfaces;

namespace Application.Users;

public class UserCommandService(IUserRepository userRepository, ITelegramTokenRepository telegramTokenRepository)
	: IUserCommandService
{
	public async Task<string> GenerateTgLinkAsync(int userId, CancellationToken ct)
	{
		var token = Convert
			.ToBase64String(RandomNumberGenerator.GetBytes(24))
			.Replace("+", "-")
			.Replace("/", "_")
			.TrimEnd('=');

		await telegramTokenRepository.SetAsync(token, userId, TimeSpan.FromMinutes(5));

		return $"https://t.me/ViaTradeBot?start={token}";
	}

	public async Task LinkTelegramAsync(string telegramToken, string telegramId, CancellationToken ct)
	{
		var userIdNullable = await telegramTokenRepository.FindUserIdAsync(telegramToken);
		if (userIdNullable == null)
			throw new InvalidTokenException("The Telegram link token is invalid or expired.");

		await telegramTokenRepository.RemoveAsync(telegramToken);
		var userId = userIdNullable.Value;

		var affectedRows = await userRepository.UpdateTelegramIdAsync(userId, telegramId, ct);
		if (affectedRows == 0)
			throw new NotFoundException("User not found.", "user_not_found");
	}

	public async Task UpdateLastLoginAtAsync(int userId, CancellationToken ct)
	{
		await userRepository.UpdateLastLoginAtAsync(userId, DateTime.UtcNow, ct);
	}
}
