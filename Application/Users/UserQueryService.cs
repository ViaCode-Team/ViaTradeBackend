using Application.Users.Interfaces;
using Application.Users.Models;

namespace Application.Users;

public class UserQueryService(IUserRepository userRepository, ITelegramTokenRepository telegramTokenRepository)
	: IUserQueryService
{
	public async Task<IReadOnlyList<UserTelegramDto>> GetTelegramRecipientsAsync(CancellationToken ct)
	{
		return await userRepository.GetTelegramRecipientsAsync(ct);
	}

	public async Task<UserMeDto?> GetMeAsync(int userId, CancellationToken ct)
	{
		return await userRepository.GetMeAsync(userId, ct);
	}

	public async Task<int?> GetIdAsync(string telegramToken, CancellationToken ct)
	{
		var userId = await telegramTokenRepository.GetUserIdAsync(telegramToken);

		if (userId == null)
			return null;

		await telegramTokenRepository.RemoveAsync(telegramToken);

		return userId;
	}
}
