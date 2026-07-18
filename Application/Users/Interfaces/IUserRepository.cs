using Application.Common.Interfaces.Repositories;
using Domain.Users.Entities;

namespace Application.Users.Interfaces;

public interface IUserRepository : IRepository<User>
{
	Task<User?> GetByLoginAsync(string login, CancellationToken cancellationToken = default);
	Task<IEnumerable<User>> GetAllWithTgLinkAsync(CancellationToken cancellationToken = default);
	Task<int> UpdateTgIdAsync(int userId, string tgId, CancellationToken cancellationToken = default);
	Task<int> UpdateLastLoginDateAsync(int userId, DateTime lastLoginDate, CancellationToken cancellationToken = default);
}
