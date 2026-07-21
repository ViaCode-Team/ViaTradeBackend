using Application.Users.Models;

namespace Application.Users.Interfaces;

public interface IUserQueryService
{
	Task<UserMeDto?> GetMeAsync(int userId, CancellationToken ct);
	Task<IReadOnlyList<UserTelegramDto>> GetTelegramRecipientsAsync(CancellationToken ct);
	Task<int?> GetIdAsync(string token, CancellationToken ct);
}
