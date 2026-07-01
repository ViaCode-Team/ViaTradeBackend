using Domain.Entities.DataBase;
using Domain.Models.Dto.User;

namespace Application.Interfaces.Repositories.Database
{
    public interface IUserRepository : IRepository<User, UserDto>
    {
        Task<User?> GetByLoginAsync(string login, CancellationToken cancellationToken = default);
        Task<IEnumerable<User>> GetAllWithTgLinkAsync(CancellationToken cancellationToken = default);
    }
}
