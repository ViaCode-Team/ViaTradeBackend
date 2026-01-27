namespace Application.Intarfaces
{
    public interface ITokenCacheRepository
    {
        Task SetAccessTokenAsync(int userId, string token, TimeSpan ttl);
        Task<string?> GetAccessTokenAsync(int userId);
        Task RemoveAccessTokenAsync(int userId);
    }
}
