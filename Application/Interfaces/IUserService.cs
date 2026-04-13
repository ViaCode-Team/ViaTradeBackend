using System.Threading;
using Domain.Entities.DataBase;

namespace Application.Interfaces
{
    public interface IUserService
    {
        Task<User> EnsureUserAsunc(int userId, CancellationToken cancellationToken);
    }
}
