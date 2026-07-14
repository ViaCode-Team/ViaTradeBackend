using Domain.Entities.DataBase;

namespace Application.Interfaces;

public interface IUserService
{
	Task<User> EnsureUserAsync(int userId, CancellationToken cancellationToken);
	Task<string> GenerateTgLink(int userId);
	Task<int?> GetUserId(string tgToken);
	Task<User?> GetByLoginAsync(string login, CancellationToken cancellationToken);
	Task<IEnumerable<User>> GetAllWithTgLinkAsync(CancellationToken cancellationToken);
	Task LinkTelegramAsync(string tgToken, string tgId, CancellationToken cancellationToken);
	Task UpdateLastLoginDateAsync(int userId, CancellationToken cancellationToken);
}
