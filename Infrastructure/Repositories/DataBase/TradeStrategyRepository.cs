using Application.Intarfaces;
using Application.Intarfaces.Database;
using Domain.Entities.DataBase;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositoryes.DataBase
{
    public class TradeStrategyRepository(AppDbContext context) : GenericRepository<TradeStrategy>(context), ITradeStrategyRepository
    {
        public async Task<TradeStrategy?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .FirstOrDefaultAsync(ts => ts.Name == name, cancellationToken);
        }
    }
}
