using Domain.Users.Entities;
using Application.Models;
using Domain.Entities.DataBase;

namespace Application.Interfaces.Repositories.Database;

public interface IUserRepository : IRepository<User>
{
	Task<User?> GetByLoginAsync(string login, CancellationToken cancellationToken = default);
	Task<IEnumerable<User>> GetAllWithTgLinkAsync(CancellationToken cancellationToken = default);
	Task<int> UpdateTgIdAsync(int userId, string tgId, CancellationToken cancellationToken = default);
	Task<int> UpdateLastLoginDateAsync(int userId, DateTime lastLoginDate, CancellationToken cancellationToken = default);
}
