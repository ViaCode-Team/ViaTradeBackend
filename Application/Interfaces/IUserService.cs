using Domain.Entities.DataBase;

namespace Application.Interfaces
{
    public interface IUserService
    {
        Task<User?> EnsureUser(int userId);
    }
}
