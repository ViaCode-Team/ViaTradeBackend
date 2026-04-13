using Domain.Entities.DataBase;

namespace Application.Interfaces.Database
{
    public interface IUserRepository
    {
        Task<User?> GetByLoginAsync(string login, CancellationToken cancellationToken = default);
    }
}
