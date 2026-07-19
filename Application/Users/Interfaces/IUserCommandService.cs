namespace Application.Users.Interfaces;

public interface IUserCommandService
{
	Task<string> GenerateTgLinkAsync(int userId, CancellationToken ct);
	Task LinkTelegramAsync(string telegramToken, string telegramId, CancellationToken ct);
	Task UpdateLastLoginDateAsync(int userId, CancellationToken ct);
}
