using System.Threading;
using Domain.Entities.DataBase;

namespace Application.Interfaces
{
    public interface IUserService
    {
        Task<User> EnsureUserAsync(int userId, CancellationToken cancellationToken);
    }
}
