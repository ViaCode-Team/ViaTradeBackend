using Application.Common.Exceptions;
using Application.Users.Interfaces;
using Application.Users.Models;

namespace Application.Users;

public class UserQueryService(IUserRepository userRepository, ITelegramTokenRepository telegramTokenRepository)
	: IUserQueryService
{
	public async Task<IReadOnlyList<UserTelegramDto>> ListTelegramRecipientsAsync(CancellationToken ct)
	{
		return await userRepository.ListTelegramRecipientsAsync(ct);
	}

	public async Task<UserMeDto> GetCurrentUserAsync(int userId, CancellationToken ct)
	{
		var user = await userRepository.FindMeAsync(userId, ct);
		if (user == null)
			throw new NotFoundException("User not found.", "user_not_found");

		return user;
	}

	public async Task<int?> FindUserIdByTelegramTokenAsync(string telegramToken, CancellationToken ct)
	{
		var userId = await telegramTokenRepository.FindUserIdAsync(telegramToken);
		if (userId == null)
			return null;

		await telegramTokenRepository.RemoveAsync(telegramToken);

		return userId;
	}
}
