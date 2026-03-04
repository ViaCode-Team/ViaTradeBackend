using Application.Interfaces.Database;
using Domain.Entities.DataBase;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositoryes.DataBase
{
    public class TradeCodeRepository(AppDbContext context) : GenericRepository<TradeCode>(context), ITradeCodeRepository
    {
        public async Task<TradeCode?> GetByExchangeIdAsync(string code, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .FirstOrDefaultAsync(tc => tc.ExchangeId == code, cancellationToken);
        }
    }
}
