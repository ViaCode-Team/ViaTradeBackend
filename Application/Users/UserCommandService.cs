using Application.Users.Interfaces;
using System.Security.Cryptography;

namespace Application.Users;

public class UserCommandService(
	IUserRepository userRepository,
	ITgTokenRepository tgTokenRepository) : IUserCommandService
{
	public async Task<string> GenerateTgLinkAsync(int userId, CancellationToken ct)
	{
		var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(24))
			.Replace("+", "-")
			.Replace("/", "_")
			.TrimEnd('=');

		await tgTokenRepository.SetAsync(token, userId, TimeSpan.FromMinutes(5));

		return $"https://t.me/ViaTradeBot?start={token}";
	}

	public async Task LinkTelegramAsync(string tgToken, string tgId, CancellationToken ct)
	{
		var userIdNullable = await tgTokenRepository.GetUserIdAsync(tgToken);
		if (userIdNullable == null)
			throw new NullReferenceException(nameof(tgToken));

		await tgTokenRepository.RemoveAsync(tgToken);
		var userId = userIdNullable.Value;

		var affectedRows = await userRepository.UpdateTgIdAsync(userId, tgId, ct);
		if (affectedRows == 0)
			throw new NullReferenceException(nameof(tgToken));
	}

	public async Task UpdateLastLoginDateAsync(int userId, CancellationToken ct)
	{
		await userRepository.UpdateLastLoginDateAsync(userId, DateTime.UtcNow, ct);
	}
}
