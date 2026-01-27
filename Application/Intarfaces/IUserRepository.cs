using Domain.Entities.DataBase;

namespace Application.Intarfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByLoginAsync(string login);
        Task AddAsync(User user);
        Task UpdateAsync(User user);
    }
}
