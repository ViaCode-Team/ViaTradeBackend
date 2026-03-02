using Domain.Entities.DataBase;

namespace Application.Intarfaces
{
    public interface IUserService
    {
        Task<User?> EnsureUser(int userId);
    }
}
