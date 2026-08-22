using ViaTrade.Application.Users.Models;

namespace ViaTrade.Application.Users.Interfaces;

public interface IUserQueryService
{
	Task<UserMeDto> GetCurrentUserAsync(int userId, CancellationToken ct);
	Task<int?> FindUserIdByTelegramTokenAsync(string token, CancellationToken ct);
}
