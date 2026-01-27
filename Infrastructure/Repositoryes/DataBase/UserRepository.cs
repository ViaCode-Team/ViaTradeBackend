using Application.Intarfaces;
using Domain.Entities.DataBase;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositoryes.DataBase
{
    public class UserRepository(AppDbContext context) : GenericRepository<User>(context), IUserRepository
    {
        public async Task<User?> GetByLoginAsync(string login)
        {
            return await _dbSet.FirstOrDefaultAsync(u => u.Login == login);
        }

        public Task UpdateAsync(User user)
        {
            throw new NotImplementedException();
        }
    }
}
