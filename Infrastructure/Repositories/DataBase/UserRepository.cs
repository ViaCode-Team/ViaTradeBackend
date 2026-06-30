using Application.Interfaces.Database;
using Domain.Entities.DataBase;
using Domain.Models.Dto.User;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.DataBase
{
    public class UserRepository(AppDbContext context) : GenericRepository<User, UserDto>(context), IUserRepository
    {
        public async Task<User?> GetByLoginAsync(string login, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .FirstOrDefaultAsync(u => u.Login == login, cancellationToken);
        }

        public async Task<IEnumerable<User>> GetAllWithTgLikn(CancellationToken cancellationToken = default)
        {
            return await _dbSet.Where(u => u.TgId != null).ToListAsync(cancellationToken);
        }
    }
}
