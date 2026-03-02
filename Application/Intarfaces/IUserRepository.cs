using Domain.Entities.DataBase;

namespace Application.Intarfaces
{
    public interface IUserRepository : IRepository<User>
    {
        Task<User?> GetByLoginAsync(string login);
        Task UpdateAsync(User user);
    }
}
