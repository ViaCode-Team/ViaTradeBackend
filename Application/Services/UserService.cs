using Application.Interfaces;
using Application.Interfaces.Repositories.Database;
using Application.Interfaces.Repositories.Redis;
using Domain.Entities.DataBase;
using Domain.Entities.Redis;
using System.Security.Cryptography;

namespace Application.Services;

public class UserService(
	IUserRepository userRepository,
	IRedisRepository<TgTokenEntity> tgTokenRepository) : IUserService
{
	private readonly IUserRepository _userRepository = userRepository;
	private readonly IRedisRepository<TgTokenEntity> _tgTokenRepository = tgTokenRepository;

	public async Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken)
	{
		return await _userRepository.GetByIdAsync(id, cancellationToken);
	}

	public async Task<string> GenerateTgLink(int userId)
	{
		var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(24))
			.Replace("+", "-")
			.Replace("/", "_")
			.TrimEnd('=');

		var entity = new TgTokenEntity
		{
			Id = token,
			UserId = userId
		};

		await _tgTokenRepository.SetAsync(entity, TimeSpan.FromMinutes(5));

		return $"https://t.me/ViaTradeBot?start={token}";
	}

	public async Task<int?> GetUserId(string tgToken)
	{
		var entity = await _tgTokenRepository.GetAsync(tgToken);

		if (entity == null)
			return null;

		await _tgTokenRepository.RemoveAsync(tgToken);

		return entity.UserId;
	}

	public async Task<User?> GetByLoginAsync(string login, CancellationToken cancellationToken)
	{
		return await _userRepository.GetByLoginAsync(login, cancellationToken);
	}

	public async Task<IEnumerable<User>> GetAllWithTgLinkAsync(CancellationToken cancellationToken)
	{
		return await _userRepository.GetAllWithTgLinkAsync(cancellationToken);
	}

	public async Task LinkTelegramAsync(string tgToken, string tgId, CancellationToken cancellationToken)
	{
		var userId = await GetUserId(tgToken)
			?? throw new NullReferenceException(nameof(tgToken));

		var affectedRows = await _userRepository.UpdateTgIdAsync(userId, tgId, cancellationToken);
		if (affectedRows == 0)
			throw new NullReferenceException(nameof(tgToken));
	}

	public async Task UpdateLastLoginDateAsync(int userId, CancellationToken cancellationToken)
	{
		await _userRepository.UpdateLastLoginDateAsync(userId, DateTime.UtcNow, cancellationToken);
	}
}
