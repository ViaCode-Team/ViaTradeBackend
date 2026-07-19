using Application.Users.Interfaces;
using Domain.Users.Entities;

namespace Application.Users;

public class UserQueryService(
	IUserRepository userRepository,
	ITelegramTokenRepository telegramTokenRepository) : IUserQueryService
{
	public async Task<IEnumerable<User>> GetWithTgLinkAsync(CancellationToken ct)
	{
		return await userRepository.GetAllWithTgLinkAsync(ct);
	}

	public async Task<User?> GetAsync(int id, CancellationToken ct)
	{
		return await userRepository.GetByIdAsync(id, ct);
	}

	public async Task<User?> GetAsync(string login, CancellationToken ct)
	{
		return await userRepository.GetByLoginAsync(login, ct);
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
