using Application.Intarfaces;
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

        public async Task<User?> GetByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .FirstOrDefaultAsync(u => u.RefreshToken == refreshToken, cancellationToken);
        }

        public async Task UpdateAsync(User user, CancellationToken cancellationToken = default)
        {
            _dbSet.Update(user);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateRefreshTokenAsync(int userId, string refreshToken, CancellationToken cancellationToken = default)
        {
            await _dbSet
                .Where(u => u.Id == userId)
                .ExecuteUpdateAsync(
                    s => s.SetProperty(u => u.RefreshToken, refreshToken),
                    cancellationToken);
        }
    }
}
