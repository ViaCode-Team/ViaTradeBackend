using Application.Interfaces.Database;
using Domain.Entities.DataBase;
using Domain.Models.Dto.Trade;
using Infrastructure.Repositoryes.DataBase;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.DataBase
{
    public class TradeRepository(AppDbContext context)
        : GenericRepository<Trade, TradeDto>(context), ITradeRepository
    {
        public async Task<IEnumerable<Trade>> GetByUserAsync(int userId, CancellationToken cancellationToken)
        {
            return await _dbSet
                .Include(t => t.TradeType)
                .Include(t => t.TradeCode)
                .Where(t => t.UserId == userId)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Trade>> GetByUserAndTradeCodeAsync(int userId, int tradeCodeId, CancellationToken cancellationToken)
        {
            return await _dbSet
                .Include(t => t.TradeType)
                .Include(t => t.TradeCode)
                .Where(t => t.UserId == userId && t.TradeCodeId == tradeCodeId)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Trade>> GetByUserAndDateRangeAsync(int userId, DateTime? from, DateTime? to, TradeSignal? tradeSignal, CancellationToken cancellationToken)
        {
            var query = _dbSet
                .Include(t => t.TradeType)
                .Include(t => t.TradeCode)
                .Where(t => t.UserId == userId);

            if (from.HasValue)
                query = query.Where(t => t.DateOpen >= from.Value);

            if (to.HasValue)
                query = query.Where(t => t.DateOpen <= to.Value.Date.AddDays(1).AddTicks(-1));

            if (tradeSignal.HasValue)
            {
                query = query.Where(t => t.TradeSignal == tradeSignal);
            }

            return await query.ToListAsync(cancellationToken);
        }
    }
}
