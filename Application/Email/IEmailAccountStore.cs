using ViaTrade.Domain.Entities;

namespace ViaTrade.Application.Email;

public interface IEmailAccountStore
{
	Task<T> ExecuteAsync<T>(Func<IEmailAccountTransaction, Task<T>> operation, CancellationToken ct);

	Task<IReadOnlyList<int>> ListMaintenanceUserIdsAsync(int afterUserId, int limit, CancellationToken ct);
}

public interface IEmailAccountTransaction
{
	Task<User?> FindByUserIdAsync(int userId, CancellationToken ct);

	Task<User?> FindByIdentityAsync(string identity, CancellationToken ct);

	Task<bool> IsEmailOccupiedAsync(string normalizedEmail, DateTime now, CancellationToken ct);

	Task AddAsync(User user, CancellationToken ct);

	void Update(User user);

	void Delete(User user);
}
