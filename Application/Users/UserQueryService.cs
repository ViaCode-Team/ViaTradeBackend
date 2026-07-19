using Application.Users.Interfaces;
using Domain.Users.Entities;

namespace Application.Users;

public class UserQueryService(
	IUserRepository userRepository,
	ITgTokenRepository tgTokenRepository) : IUserQueryService
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

	public async Task<int?> GetIdAsync(string tgToken, CancellationToken ct)
	{
		var userId = await tgTokenRepository.GetUserIdAsync(tgToken);

		if (userId == null)
			return null;

		await tgTokenRepository.RemoveAsync(tgToken);

		return userId;
	}
}
