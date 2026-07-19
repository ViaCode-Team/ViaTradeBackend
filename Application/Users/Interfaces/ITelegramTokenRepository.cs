namespace Application.Users.Interfaces;

public interface ITelegramTokenRepository
{
	Task SetAsync(string token, int userId, TimeSpan expiry);
	Task<int?> GetUserIdAsync(string token);
	Task RemoveAsync(string token);
}
