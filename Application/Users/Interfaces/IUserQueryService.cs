using Application.Users.Models;

namespace Application.Users.Interfaces;

public interface IUserQueryService
{
	Task<UserMeDto> GetCurrentUserAsync(int userId, CancellationToken ct);
	Task<IReadOnlyList<UserTelegramDto>> ListTelegramRecipientsAsync(CancellationToken ct);
	Task<int?> FindUserIdByTelegramTokenAsync(string token, CancellationToken ct);
}
