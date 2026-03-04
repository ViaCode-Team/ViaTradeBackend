using Application.Interfaces.Database;
using Domain.Entities.DataBase;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositoryes.DataBase
{
    public class UserRepository(AppDbContext context) : GenericRepository<User>(context), IUserRepository
    {
        public async Task<User?> GetByLoginAsync(string login, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .FirstOrDefaultAsync(u => u.Login == login, cancellationToken);
        }

        public async Task UpdateAsync(User user, CancellationToken cancellationToken = default)
        {
            _dbSet.Update(user);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
