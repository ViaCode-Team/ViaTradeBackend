using ViaTrade.Application.Common.Exceptions;
using ViaTrade.Application.Common.Interfaces.Repositories;
using ViaTrade.Application.Users.Interfaces;
using ViaTrade.Application.Users.Models;
using ViaTrade.Domain.Entities;

namespace ViaTrade.Application.Users;

public class UserQueryService(
	IReadRepository<User> userRepository,
	ICacheRepository<TelegramTokenEntity> telegramTokenRepository
) : IUserQueryService
{
	public async Task<UserMeDto> GetCurrentUserAsync(int userId, CancellationToken ct)
	{
		var user = await userRepository.FirstOrDefaultAsync(
			user => user.Id == userId,
			user => new UserMeDto
			{
				Id = user.Id,
				Login = user.Login,
				LastLoginAt = user.LastLoginAt,
				RegisteredAt = user.RegisteredAt,
				TelegramId = user.TelegramId,
			},
			ct
		);
		if (user == null)
			throw new NotFoundException("User not found.", "user_not_found");

		return user;
	}

	public async Task<int?> FindUserIdByTelegramTokenAsync(string telegramToken, CancellationToken ct)
	{
		var token = await telegramTokenRepository.FindByIdAsync(telegramToken);
		if (token == null)
			return null;

		await telegramTokenRepository.RemoveAsync(telegramToken);

		return token.UserId;
	}
}
