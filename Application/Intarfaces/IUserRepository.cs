using Domain.Entities.DataBase;

namespace Application.Intarfaces
{
    public interface IUserRepository : IRepository<User>
    {
        Task<User?> GetByLoginAsync(string login);
        Task<User?> GetByRefreshTokenAsync(string refreshToken);
        Task UpdateAsync(User user);
        Task UpdateRefreshTokenAsync(int userId, string refreshToken);
    }
}
