using Domain.Entities.DataBase;

namespace Application.Interfaces.Database
{
    public interface IUserRepository : IRepository<User>
    {
        Task<User?> GetByLoginAsync(string login, CancellationToken cancellationToken = default);
        Task UpdateAsync(User user, CancellationToken cancellationToken = default);
    }
}
