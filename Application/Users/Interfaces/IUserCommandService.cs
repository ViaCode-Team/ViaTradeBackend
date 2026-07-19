namespace Application.Users.Interfaces;

public interface IUserCommandService
{
	Task<string> GenerateTgLinkAsync(int userId, CancellationToken ct);
	Task LinkTelegramAsync(string tgToken, string tgId, CancellationToken ct);
	Task UpdateLastLoginDateAsync(int userId, CancellationToken ct);
}
