namespace Application.Intarfaces
{
    public interface IRefreshTokenRepository
    {
        Task StoreAsync(string sessionId, string refreshToken);
        Task<string?> GetSessionIdAsync(string refreshToken);
        Task RotateAsync(string sessionId, string newRefreshToken);
        Task RemoveAsync(string sessionId);
    }
}
