using Domain.Users.Entities;

namespace Application.Users.Interfaces;

public interface IUserQueryService
{
	Task<IEnumerable<User>> GetWithTgLinkAsync(CancellationToken ct);
	Task<User?> GetAsync(int id, CancellationToken ct);
	Task<User?> GetAsync(string login, CancellationToken ct);
	Task<int?> GetIdAsync(string token, CancellationToken ct);
}
