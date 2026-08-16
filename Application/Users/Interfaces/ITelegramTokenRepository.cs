namespace Application.Users.Interfaces;

public interface ITelegramTokenRepository
{
	Task SetAsync(string token, int userId, TimeSpan expiry);
	Task<int?> FindUserIdAsync(string token);
	Task<int?> ConsumeUserIdAsync(string token);
	Task RemoveAsync(string token);
}
